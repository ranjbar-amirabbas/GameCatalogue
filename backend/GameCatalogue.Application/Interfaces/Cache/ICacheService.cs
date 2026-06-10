namespace GameCatalogue.Application.Interfaces.Cache;

/// <summary>
/// Abstraction over a distributed cache.
/// </summary>
public interface ICacheService
{
    /// <summary>Gets a cached value by key.</summary>
    /// <typeparam name="T">The reference type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The cached value, or <c>null</c> if the key is absent.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct) where T : class;

    /// <summary>Stores a value in the cache with a time-to-live.</summary>
    /// <typeparam name="T">The reference type of the value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttl">The time-to-live for the cache entry.</param>
    /// <param name="ct">A cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct) where T : class;

    /// <summary>Removes a single cache entry by key.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">A cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken ct);

    /// <summary>Removes all cache entries matching a glob pattern.</summary>
    /// <param name="pattern">The key pattern (e.g. <c>games:*</c>).</param>
    /// <param name="ct">A cancellation token.</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken ct);
}
