using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Services;
using System.Collections.Concurrent;

namespace PowerBillingUsage.Infrastructure.Services;

public class CacheService : ICacheService, IScopedDependency
{
    private static readonly TimeSpan _defualtExpiration = TimeSpan.FromMinutes(5);

    private static readonly ConcurrentDictionary<string, bool> _cachekeys = new();

    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? cacheItem = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (cacheItem is null)
            return default;

        var result = JsonConvert.DeserializeObject<T>(cacheItem);
        if (result is null)
            return default;

        return result;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        T? cacheItem = await GetAsync<T>(key, cancellationToken);
        if (cacheItem is not null)
            return cacheItem;

        cacheItem = await factory(cancellationToken);

        await SetAsync(key, cacheItem, expiration, cancellationToken);

        return cacheItem;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);

        _cachekeys.TryRemove(key, out bool _);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        IEnumerable<Task> tasks = _cachekeys.Keys
            .Where(key => key.StartsWith(prefixKey))
            .Select(key => RemoveAsync(key, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        string cacheItem = JsonConvert.SerializeObject(value);

        await _distributedCache.SetStringAsync(
            key,
            cacheItem,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime ?? _defualtExpiration
            },
            cancellationToken
        );

        _cachekeys.TryAdd(key, false);
    }
}