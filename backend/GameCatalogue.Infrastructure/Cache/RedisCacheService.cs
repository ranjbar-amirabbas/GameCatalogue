using System.Text.Json;
using GameCatalogue.Application.Interfaces.Cache;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GameCatalogue.Infrastructure.Cache;

/// <summary>
/// Redis-backed implementation of <see cref="ICacheService"/> using JSON serialization.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger)
    {
        _connection = connection;
        _db = connection.GetDatabase();
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>((string)value!, SerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cached value for {Key}", key);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct) where T : class
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        await _db.StringSetAsync(key, json, ttl);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        await _db.KeyDeleteAsync(key);
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct)
    {
        foreach (var endpoint in _connection.GetEndPoints())
        {
            var server = _connection.GetServer(endpoint);
            if (!server.IsConnected || server.IsReplica)
            {
                continue;
            }

            var keys = server.Keys(database: _db.Database, pattern: pattern, pageSize: 250);
            foreach (var key in keys)
            {
                ct.ThrowIfCancellationRequested();
                await _db.KeyDeleteAsync(key);
            }
        }

        _logger.LogInformation("Invalidated cache keys matching pattern {Pattern}", pattern);
    }
}
