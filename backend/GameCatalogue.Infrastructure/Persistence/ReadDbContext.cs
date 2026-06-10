using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameCatalogue.Infrastructure.Persistence;

/// <summary>
/// Read-side EF Core context. Exposes a no-tracking queryable over games.
/// </summary>
public class ReadDbContext : DbContext, IReadDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadDbContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
    }

    /// <inheritdoc />
    public IQueryable<Game> Games => Set<Game>().AsNoTracking();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
