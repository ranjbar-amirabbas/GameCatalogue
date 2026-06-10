using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="GameUpdatedEvent"/> by invalidating the specific game
/// cache and all list caches.
/// </summary>
public class GameUpdatedEventHandler : INotificationHandler<GameUpdatedEvent>
{
    private readonly ICacheService _cache;
    private readonly ILogger<GameUpdatedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameUpdatedEventHandler"/> class.
    /// </summary>
    public GameUpdatedEventHandler(ICacheService cache, ILogger<GameUpdatedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(GameUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GameUpdatedEvent for {GameId} ({Title})",
            notification.GameId,
            notification.Title);

        await _cache.RemoveAsync($"game:{notification.GameId}", cancellationToken);
        await _cache.RemoveByPatternAsync("games:*", cancellationToken);
    }
}
