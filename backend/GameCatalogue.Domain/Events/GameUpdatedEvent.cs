namespace GameCatalogue.Domain.Events;

/// <summary>
/// Domain event raised when an existing game is updated.
/// </summary>
/// <param name="GameId">The identifier of the updated game.</param>
/// <param name="Title">The title of the updated game.</param>
public record GameUpdatedEvent(Guid GameId, string Title) : IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
