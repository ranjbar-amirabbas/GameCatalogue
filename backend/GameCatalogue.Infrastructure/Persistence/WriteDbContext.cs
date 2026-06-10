using System.Text.Json;
using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Events;
using GameCatalogue.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameCatalogue.Infrastructure.Persistence;

/// <summary>
/// Write-side EF Core context. On save it (1) persists pending domain events as
/// <see cref="OutboxMessage"/> rows within the same transaction (outbox pattern,
/// at-least-once delivery) and (2) dispatches them in-process via MediatR right
/// after the commit, so reads immediately reflect the change.
/// </summary>
public class WriteDbContext : DbContext, IWriteDbContext
{
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="WriteDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    /// <param name="publisher">The MediatR publisher used to dispatch domain events.</param>
    public WriteDbContext(DbContextOptions<WriteDbContext> options, IPublisher publisher) : base(options)
    {
        _publisher = publisher;
    }

    /// <summary>Gets the games set.</summary>
    public DbSet<Game> Games => Set<Game>();

    /// <summary>Gets the outbox messages set.</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Drain domain events from the tracked aggregates (also clears them).
        var domainEvents = CollectDomainEvents();

        // Persist them as outbox messages in the same transaction as the change.
        AddOutboxMessages(domainEvents);

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch in-process after the commit for read-your-writes consistency.
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }

    private void AddOutboxMessages(IReadOnlyList<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().FullName!,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredAt = domainEvent.OccurredAt,
                ProcessedAt = null,
                Error = null
            });
        }
    }
}
