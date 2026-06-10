using GameCatalogue.Domain.Events;

namespace GameCatalogue.Domain.Entities;

/// <summary>
/// Base class for aggregate roots. Tracks domain events raised during the
/// lifetime of the aggregate so they can be dispatched after persistence.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the read-only collection of domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event to be dispatched once the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all recorded domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
