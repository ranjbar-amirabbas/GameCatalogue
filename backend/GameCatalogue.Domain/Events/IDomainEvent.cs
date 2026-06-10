using MediatR;

namespace GameCatalogue.Domain.Events;

/// <summary>
/// Marker interface for domain events raised by aggregate roots. Extends
/// <see cref="INotification"/> so events can be dispatched through MediatR.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the UTC timestamp at which the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}
