using GameCatalogue.Domain.Entities;

namespace GameCatalogue.Domain.Interfaces;

/// <summary>
/// Write-side repository abstraction for the <see cref="Game"/> aggregate.
/// </summary>
public interface IGameWriteRepository
{
    /// <summary>Adds a new game and persists the change.</summary>
    /// <param name="game">The game to add.</param>
    /// <param name="ct">A cancellation token.</param>
    Task AddAsync(Game game, CancellationToken ct);

    /// <summary>Updates an existing game and persists the change.</summary>
    /// <param name="game">The game to update.</param>
    /// <param name="ct">A cancellation token.</param>
    Task UpdateAsync(Game game, CancellationToken ct);

    /// <summary>Deletes a game and persists the change.</summary>
    /// <param name="game">The game to delete.</param>
    /// <param name="ct">A cancellation token.</param>
    Task DeleteAsync(Game game, CancellationToken ct);

    /// <summary>Retrieves a game by its identifier.</summary>
    /// <param name="id">The identifier of the game.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The game if found; otherwise <c>null</c>.</returns>
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct);
}
