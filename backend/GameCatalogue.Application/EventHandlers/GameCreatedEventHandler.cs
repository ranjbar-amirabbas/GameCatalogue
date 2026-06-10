using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="GameCreatedEvent"/> by invalidating list caches.
/// </summary>
public class GameCreatedEventHandler : INotificationHandler<GameCreatedEvent>
{
    private readonly ICacheService _cache;
    private readonly ILogger<GameCreatedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameCreatedEventHandler"/> class.
    /// </summary>
    public GameCreatedEventHandler(ICacheService cache, ILogger<GameCreatedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(GameCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GameCreatedEvent for {GameId} ({Title})",
            notification.GameId,
            notification.Title);

        await _cache.RemoveByPatternAsync("games:*", cancellationToken);
    }
}
