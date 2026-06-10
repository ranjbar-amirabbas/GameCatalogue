using System.Text.Json;
using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Domain.Entities;
using GameCatalogue.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace GameCatalogue.Infrastructure.Persistence;

/// <summary>
/// Write-side EF Core context. On save it converts pending domain events into
/// <see cref="OutboxMessage"/> rows within the same transaction (outbox pattern).
/// </summary>
public class WriteDbContext : DbContext, IWriteDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WriteDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options)
    {
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
        ConvertDomainEventsToOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var message = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().FullName!,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredAt = domainEvent.OccurredAt,
                    ProcessedAt = null,
                    Error = null
                };

                OutboxMessages.Add(message);
            }

            aggregate.ClearDomainEvents();
        }
    }
}
