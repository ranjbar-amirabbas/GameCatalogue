using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameCatalogue.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools to create a
/// <see cref="WriteDbContext"/> without booting the full application host.
/// </summary>
public class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    /// <inheritdoc />
    public WriteDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("WRITE_CONNECTION")
            ?? "Server=localhost,1433;Database=GameCatalogue;User Id=sa;Password=GameCatalogue_2024!;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        // Migrations never raise/dispatch domain events, so a no-op publisher is fine.
        return new WriteDbContext(options, new NoOpPublisher());
    }

    private sealed class NoOpPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
