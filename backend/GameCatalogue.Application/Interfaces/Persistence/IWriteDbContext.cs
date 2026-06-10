using GameCatalogue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameCatalogue.Application.Interfaces.Persistence;

/// <summary>
/// Abstraction over the write-side database context.
/// </summary>
public interface IWriteDbContext
{
    /// <summary>Gets the games set.</summary>
    DbSet<Game> Games { get; }

    /// <summary>Persists all pending changes.</summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
