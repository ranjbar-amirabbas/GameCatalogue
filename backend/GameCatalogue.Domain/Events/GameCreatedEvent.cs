namespace GameCatalogue.Domain.Events;

/// <summary>
/// Domain event raised when a new game is created.
/// </summary>
/// <param name="GameId">The identifier of the created game.</param>
/// <param name="Title">The title of the created game.</param>
public record GameCreatedEvent(Guid GameId, string Title) : IDomainEvent
{
    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
