using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameCatalogue.Application.EventHandlers;

/// <summary>
/// Reacts to <see cref="GameDeletedEvent"/> by invalidating the specific game
/// cache and all list caches.
/// </summary>
public class GameDeletedEventHandler : INotificationHandler<GameDeletedEvent>
{
    private readonly ICacheService _cache;
    private readonly ILogger<GameDeletedEventHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameDeletedEventHandler"/> class.
    /// </summary>
    public GameDeletedEventHandler(ICacheService cache, ILogger<GameDeletedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Handle(GameDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GameDeletedEvent for {GameId} ({Title})",
            notification.GameId,
            notification.Title);

        await _cache.RemoveAsync($"game:{notification.GameId}", cancellationToken);
        await _cache.RemoveByPatternAsync("games:*", cancellationToken);
    }
}
