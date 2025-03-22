using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PowerBillingUsage.Domain.Abstractions;
using System.Collections.Concurrent;

namespace PowerBillingUsage.Infrastructure.Services;

public class CacheService : ICacheService
{
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

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken = default)
    {
        T? cacheItem = await GetAsync<T>(key, cancellationToken);
        if (cacheItem is not null)
            return cacheItem;

        cacheItem = await factory();

        await SetAsync(key, cacheItem, cancellationToken);

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

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        string cacheItem = JsonConvert.SerializeObject(value);

        await _distributedCache.SetStringAsync(
            key,
            cacheItem,
            cancellationToken
        );

        _cachekeys.TryAdd(key, false);
    }
}