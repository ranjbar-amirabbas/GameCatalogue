namespace GameCatalogue.Domain.Events;

/// <summary>
/// Domain event raised when a game is deleted.
/// </summary>
/// <param name="GameId">The identifier of the deleted game.</param>
/// <param name="Title">The title of the deleted game.</param>
public record GameDeletedEvent(Guid GameId, string Title) : IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
