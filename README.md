# Game Catalogue

A production-grade Video Game Catalogue built with **.NET 10** (Clean Architecture +
DDD + CQRS-lite + Outbox) and an **Angular 20** front end, fully containerised with
Docker Compose. SQL Server is the system of record, Redis provides caching, and MinIO
stores cover images.

---

## Architecture Overview

The backend follows **Clean Architecture**: dependencies point inward, and the Domain
has no knowledge of infrastructure concerns.

```
┌──────────────────────────────────────────────────────────────┐
│                          API (Web)                             │
│   Controllers · Global Exception Middleware · Swagger · CORS   │
│   Serilog request logging · API Versioning · Health checks     │
└───────────────────────────────┬──────────────────────────────┘
                                 │ depends on
┌───────────────────────────────▼──────────────────────────────┐
│                        Application                             │
│  Commands / Queries (MediatR) · DTOs · Validators             │
│  Pipeline Behaviours (Logging, Validation) · Event Handlers   │
│  Interfaces: IReadDbContext, IWriteDbContext, ICacheService,   │
│              IStorageService                                   │
└───────────────────────────────┬──────────────────────────────┘
                                 │ depends on
┌───────────────────────────────▼──────────────────────────────┐
│                          Domain                               │
│  Game aggregate · AggregateRoot · Domain Events · Enums       │
│  DomainException · IGameWriteRepository                        │
│  (no outward dependencies — pure business rules)              │
└───────────────────────────────▲──────────────────────────────┘
                                 │ implements interfaces
┌───────────────────────────────┴──────────────────────────────┐
│                       Infrastructure                          │
│  EF Core WriteDbContext / ReadDbContext · Repositories        │
│  Redis cache · MinIO storage · Outbox processor (BackgroundSvc)│
│  Serilog config · Health checks                               │
└──────────────────────────────────────────────────────────────┘
```

### Layer responsibilities

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Enterprise rules. The `Game` aggregate enforces invariants and raises domain events. No framework dependencies (only `MediatR.Contracts` for the `INotification` marker). |
| **Application** | Use cases via MediatR commands/queries, validation, caching policy, event handling, and the abstractions (`I*DbContext`, `ICacheService`, `IStorageService`) the outer layers implement. |
| **Infrastructure** | Concrete implementations: EF Core, Redis, MinIO, the transactional Outbox processor, Serilog, and health checks. |
| **API** | HTTP surface: thin controllers that only dispatch through MediatR, RFC 7807 error handling, versioning, Swagger and CORS. |

### Data flow

**Write path** (e.g. create a game)

1. `POST /api/v1/games` → `GamesController` → `IMediator.Send(CreateGameCommand)`.
2. `LoggingBehaviour` and `ValidationBehaviour` run first (FluentValidation).
3. `CreateGameCommandHandler` calls `Game.Create(...)`, which validates invariants and
   raises a `GameCreatedEvent`.
4. The repository saves the aggregate. Inside `WriteDbContext.SaveChangesAsync`, pending
   domain events are serialised into the **OutboxMessages** table in the *same
   transaction* as the entity change.
5. `WriteDbContext.SaveChangesAsync` is the single place that handles domain events: it
   drains them from the tracked aggregates, writes them to the outbox in the same
   transaction, and — right after the commit — dispatches them **in-process** via MediatR
   for read-your-writes consistency (so a list/detail re-fetch immediately reflects the
   change). Command handlers therefore never publish events themselves. The
   `OutboxProcessor` background service additionally polls the outbox every 10 seconds and
   re-publishes each event (at-least-once delivery, even if the process crashes between
   commit and dispatch). Cache-invalidation handlers are idempotent, so the double
   dispatch is harmless.
6. Event handlers (`GameCreatedEventHandler`, `GameUpdatedEventHandler`,
   `GameDeletedEventHandler`) invalidate the relevant Redis cache entries. Deleting a
   game also removes its cover image from object storage.

**Read path** (e.g. list / get games)

1. `GET /api/v1/games` → `GetGamesQueryHandler`.
2. A deterministic cache key is built from all query parameters; Redis is checked first.
3. On a miss, the query runs against `ReadDbContext` (always `AsNoTracking`), projects
   straight to a DTO, and the result is cached (5 min for lists, 10 min for a single game).

---

## Design Decisions

- **Why Clean Architecture** — keeps business rules independent of frameworks and
  databases, makes the core unit-testable without I/O, and lets infrastructure choices
  change without touching the Domain.

- **Why DDD + Domain Events** — the `Game` aggregate owns its invariants (private
  setters, validated factory/`Update` methods), so the model can never enter an invalid
  state. Domain events decouple side-effects (cache invalidation) from the core use case.

- **Why the Outbox Pattern** — publishing events in-process after a commit risks losing
  them if the process crashes. Writing events to an outbox table *in the same DB
  transaction* as the state change guarantees they are never lost; the background
  processor then delivers them with **at-least-once** semantics.

- **Why CQRS-lite (read/write DbContext separation)** — reads and writes have different
  needs. `WriteDbContext` tracks changes and runs the outbox logic; `ReadDbContext` is
  no-tracking and projects directly to DTOs for fast, allocation-light queries. This also
  makes it trivial to later point reads at a replica.

- **Why MediatR + Pipeline Behaviours** — controllers stay thin (dispatch only).
  Cross-cutting concerns (logging, validation) are implemented once as behaviours and
  apply uniformly to every request.

- **Why Redis for caching** — list and detail queries dominate read traffic. A
  distributed cache absorbs that load, survives app restarts, and is shared across
  instances. TTLs plus event-driven invalidation keep data fresh.

- **Why MinIO for object storage** — cover images are binary blobs that do not belong in
  a relational DB. MinIO is S3-compatible, so the same code works against AWS S3 in
  production by swapping configuration.

- **Read/Write connection string separation** — the two contexts use independent
  connection strings (`WriteConnection` / `ReadConnection`). Today they point at the same
  SQL Server, but the separation is a seam: reads can be redirected to a read replica
  without any code change.

---

## Tech Stack

**Backend**
- .NET 10 / ASP.NET Core
- Entity Framework Core 10 (SQL Server provider)
- MediatR (CQRS + in-process events)
- FluentValidation (+ DI extensions)
- StackExchange.Redis
- MinIO .NET SDK
- Serilog (Console sink, structured logging)
- Asp.Versioning (API versioning) + Swashbuckle (Swagger)
- AspNetCore.HealthChecks (SQL Server, Redis) + custom MinIO check

**Frontend**
- Angular 20 (standalone components)
- ng-bootstrap + Bootstrap 5
- RxJS

**Infrastructure**
- SQL Server 2022, Redis 7, MinIO
- Docker & Docker Compose
- nginx (serves the SPA, reverse-proxies `/api`)

**Testing**
- xUnit, Moq, FluentAssertions, EF Core InMemory

---

## How to Run

> Requires Docker and Docker Compose.

**Step by step**

```bash
# 1. Infrastructure (SQL Server, Redis, MinIO)
docker-compose -f docker-compose.infrastructure.yml up -d

# 2. Backend API
docker-compose -f docker-compose.backend.yml up -d --build

# 3. Frontend
docker-compose -f docker-compose.frontend.yml up -d --build
```

**Or everything at once**

```bash
docker-compose up -d --build
```

The API applies EF Core migrations on startup and retries until SQL Server is ready, so
startup ordering is handled automatically.

> **macOS note:** port **5000** is taken by the system "AirPlay Receiver" (Control
> Center). The API host port is configurable — start with a free port like so:
>
> ```bash
> API_PORT=5080 docker-compose up -d --build
> ```
>
> Then the API is at `http://localhost:5080`. The frontend is unaffected (nginx proxies
> to the API over the internal Docker network, not the host port).

| Service | URL |
|---------|-----|
| Frontend (Angular) | http://localhost:4200 |
| API + Swagger UI | http://localhost:5000/swagger (or `:${API_PORT}`) |
| Health dashboard (HTML) | http://localhost:5000/health-ui (or `:${API_PORT}`) |
| Health checks (JSON) | http://localhost:5000/health (or `:${API_PORT}`) |
| MinIO Console | http://localhost:9001 (minioadmin / minioadmin) |

### Running locally without Docker

Start the infrastructure compose file, then:

```bash
cd backend
dotnet run --project GameCatalogue.API        # uses appsettings.Development.json (localhost)

cd ../frontend/game-catalogue
npm install
npm start                                      # http://localhost:4200
```

---

## API Endpoints

Base path: `/api/v1/games`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/games` | List games. Query params: `page`, `pageSize`, `genre`, `platform`, `searchTerm`. Returns a `PagedResult`. |
| `GET` | `/api/v1/games/{id}` | Get a single game by id. `404` if not found. |
| `POST` | `/api/v1/games` | Create a game (`multipart/form-data`: game fields + optional `file` cover image, uploaded during creation). Returns `201 Created` with the new id. |
| `PUT` | `/api/v1/games/{id}` | Update a game. Returns `204 No Content`. |
| `DELETE` | `/api/v1/games/{id}` | Delete a game. Returns `204 No Content`. |
| `POST` | `/api/v1/games/{id}/cover-image` | Upload/replace a cover image (`multipart/form-data`, field `file`). Returns `{ "url": "..." }`. |
| `GET` | `/api/v1/games/images/{fileKey}` | Stream a cover image by its storage key. Optional `?w=` returns a resized thumbnail. `404` if not found. |

### Cover images

The `Game` aggregate stores only the **storage object key** (`CoverImageKey`) — never a
URL. Images live in MinIO and are never exposed publicly; they are served exclusively
through the API (`GET /api/v1/games/images/{fileKey}`). When a DTO is built, the public
URL is computed as **`{apiBaseUrl}/api/v1/games/images/{fileKey}`**, where `apiBaseUrl`
is derived from the incoming request so the URL is reachable from whatever host the
client used (direct, or via the nginx reverse proxy). Read caches store the key, not the
URL, so cached entries stay host-independent.

**Thumbnails.** The list view doesn't need full-resolution covers, so the list DTO points
at a resized thumbnail (`…/images/{fileKey}?w=160`) while the detail view uses the full
image. Resizing is done on the fly with SixLabors.ImageSharp, preserving aspect ratio and
format (no upscaling). Resized thumbnails are cached in Redis (1 h) so they're only
generated once, and the image response carries `Cache-Control: public, max-age=86400` for
browser caching. In practice an ~1.9 MB cover is served to the list as a ~77 KB thumbnail.

Errors are returned as **RFC 7807 ProblemDetails**:

| Exception | HTTP status |
|-----------|-------------|
| `NotFoundException` | 404 Not Found |
| `ValidationException` | 400 Bad Request (with an `errors` dictionary) |
| `DomainException` | 422 Unprocessable Entity |
| Unhandled | 500 Internal Server Error |

---

## Health Check

```
GET /health        # JSON: overall status + each dependency
GET /health-ui     # HTML dashboard (auto-refreshes every 5s)
```

Aggregates the health of **SQL Server**, **Redis**, and **MinIO** (the MinIO check lists
buckets). `/health` returns `200` with detailed JSON when healthy and `503` when any
dependency is down. `/health-ui` is a self-contained HTML page (served from the API's
`wwwroot`) that renders each component's status, duration and any error, and refreshes
automatically.

---

## Testing

```bash
cd backend
dotnet test
```

The suite covers Domain invariants, Application handlers and the validation behaviour
(with EF Core InMemory and Moq), and the API controller.

### TDD approach

Each layer was built test-first: the test describing the desired behaviour was written
before the implementation, then the production code was added to make it pass (red →
green), keeping the design driven by required behaviour rather than the other way around.

---

## What I Would Add in Production

- **Authentication & authorization** — JWT bearer auth, role-based policies on mutating
  endpoints.
- **Distributed tracing** — OpenTelemetry traces/metrics exported to an OTLP collector.
- **Hardened outbox** — Polly retry/backoff with a dead-letter queue and idempotent
  consumers, plus failure alerting.
- **CDN in front of MinIO/S3** — serve cover images from a CDN with cache headers and
  signed URLs.
- **Kubernetes deployment** — Helm charts, liveness/readiness probes wired to `/health`,
  horizontal pod autoscaling.
- **Integration & end-to-end tests** — `WebApplicationFactory` + Testcontainers for SQL
  Server, Redis and MinIO; Cypress/Playwright for the UI.
