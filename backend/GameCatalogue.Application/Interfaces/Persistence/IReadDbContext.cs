using GameCatalogue.Domain.Entities;

namespace GameCatalogue.Application.Interfaces.Persistence;

/// <summary>
/// Read-only abstraction over the read-side database context. All queries
/// are executed with no change tracking.
/// </summary>
public interface IReadDbContext
{
    /// <summary>Gets a no-tracking queryable over the games.</summary>
    IQueryable<Game> Games { get; }
}
