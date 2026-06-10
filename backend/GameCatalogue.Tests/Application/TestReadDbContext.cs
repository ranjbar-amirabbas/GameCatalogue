using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameCatalogue.Tests.Application;

/// <summary>
/// Minimal EF Core InMemory-backed context used to exercise read-side handlers.
/// </summary>
public class TestReadDbContext : DbContext, IReadDbContext
{
    public TestReadDbContext(DbContextOptions<TestReadDbContext> options) : base(options)
    {
    }

    public DbSet<Game> GamesSet => Set<Game>();

    IQueryable<Game> IReadDbContext.Games => Set<Game>().AsNoTracking();

    public static TestReadDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<TestReadDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TestReadDbContext(options);
    }
}
