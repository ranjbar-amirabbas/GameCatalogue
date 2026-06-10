using System.Text.Json;
using GameCatalogue.Domain.Events;
using GameCatalogue.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Infrastructure.Outbox;

/// <summary>
/// Background service that polls the outbox table and dispatches unprocessed
/// domain events through MediatR, providing at-least-once delivery.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxProcessor"/> class.
    /// </summary>
    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing the outbox.");
            }

            try
            {
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Outbox processor stopping.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = typeof(IDomainEvent).Assembly.GetType(message.Type);
                if (eventType is null)
                {
                    message.Error = $"Could not resolve event type '{message.Type}'.";
                    _logger.LogWarning("Could not resolve event type {Type}", message.Type);
                    continue;
                }

                var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Content, eventType);
                if (domainEvent is null)
                {
                    message.Error = "Deserialized event was null.";
                    continue;
                }

                await publisher.Publish(domainEvent, ct);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
