using GameCatalogue.Domain.Entities;
using GameCatalogue.Domain.Interfaces;
using GameCatalogue.Infrastructure.Persistence;

namespace GameCatalogue.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IGameWriteRepository"/>. Each operation
/// is persisted immediately via <see cref="WriteDbContext.SaveChangesAsync"/>.
/// </summary>
public class GameWriteRepository : IGameWriteRepository
{
    private readonly WriteDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameWriteRepository"/> class.
    /// </summary>
    /// <param name="context">The write context.</param>
    public GameWriteRepository(WriteDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(Game game, CancellationToken ct)
    {
        await _context.Games.AddAsync(game, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Game game, CancellationToken ct)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Game game, CancellationToken ct)
    {
        _context.Games.Remove(game);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Games.FindAsync(new object?[] { id }, ct);
    }
}
