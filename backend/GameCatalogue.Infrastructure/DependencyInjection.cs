using GameCatalogue.Application.Interfaces.Cache;
using GameCatalogue.Application.Interfaces.Persistence;
using GameCatalogue.Application.Interfaces.Storage;
using GameCatalogue.Domain.Interfaces;
using GameCatalogue.Infrastructure.Cache;
using GameCatalogue.Infrastructure.HealthChecks;
using GameCatalogue.Infrastructure.Outbox;
using GameCatalogue.Infrastructure.Persistence;
using GameCatalogue.Infrastructure.Repositories;
using GameCatalogue.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using StackExchange.Redis;

namespace GameCatalogue.Infrastructure;

/// <summary>
/// Dependency injection registrations for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence, caching, storage, the outbox processor and health checks.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var writeConnection = config.GetConnectionString("WriteConnection");
        var readConnection = config.GetConnectionString("ReadConnection");
        var redisConnection = config.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is not configured.");

        // EF Core - Write
        services.AddDbContext<WriteDbContext>(options =>
            options.UseSqlServer(writeConnection));

        // EF Core - Read
        services.AddDbContext<ReadDbContext>(options =>
            options.UseSqlServer(readConnection)
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        // Interface bindings
        services.AddScoped<IWriteDbContext>(sp => sp.GetRequiredService<WriteDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ReadDbContext>());
        services.AddScoped<IGameWriteRepository, GameWriteRepository>();

        // Redis (tolerate the cache being temporarily unavailable at startup)
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnection);
            redisOptions.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(redisOptions);
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // MinIO
        services.Configure<MinioSettings>(config.GetSection("MinIO"));
        services.AddSingleton<IMinioClient>(sp =>
        {
            var settings = config.GetSection("MinIO").Get<MinioSettings>()
                ?? new MinioSettings();
            return new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.UseSsl)
                .Build();
        });
        services.AddScoped<IStorageService, MinioStorageService>();
        services.AddSingleton<IImageResizer, ImageSharpResizer>();

        // Outbox
        services.AddHostedService<OutboxProcessor>();

        // Health checks
        services.AddHealthChecks()
            .AddSqlServer(writeConnection ?? string.Empty, name: "sqlserver")
            .AddRedis(redisConnection, name: "redis")
            .AddCheck<MinioHealthCheck>("minio");

        return services;
    }
}
