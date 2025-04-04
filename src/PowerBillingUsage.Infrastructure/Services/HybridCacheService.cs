using Microsoft.Extensions.Caching.Hybrid;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Services;

namespace PowerBillingUsage.Infrastructure.Services;

public class HybridCacheService : IHybridCacheService, IScopedDependency
{
    private static readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan _defaultLocalExpiration = TimeSpan.FromMinutes(1);

    private readonly HybridCache _hybridCache;

    public HybridCacheService(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryFlags flags = HybridCacheEntryFlags.None,
        IEnumerable<string>? tags = null,
        TimeSpan? expiration = null,
        TimeSpan? localExpiration = null,
        CancellationToken cancellationToken = default)
    {
        T? cacheItem = await _hybridCache.GetOrCreateAsync<T>(key, factory, new HybridCacheEntryOptions
        {
            Expiration = expiration ?? _defaultExpiration,
            LocalCacheExpiration = localExpiration ?? _defaultLocalExpiration,
            Flags = flags
        }, tags, cancellationToken);

        if (cacheItem is not null)
            return cacheItem;

        cacheItem = await factory(cancellationToken);

        await SetAsync(key, cacheItem, flags, tags, expiration, localExpiration, cancellationToken);

        return cacheItem;
    }

    public async ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryFlags flags = HybridCacheEntryFlags.None,
        IEnumerable<string>? tags = null,
        TimeSpan? expiration = null,
        TimeSpan? localExpiration = null,
        CancellationToken cancellationToken = default)
    {
        await _hybridCache.SetAsync<T>(key, value, new HybridCacheEntryOptions
        {
            Expiration = expiration ?? _defaultExpiration,
            LocalCacheExpiration = localExpiration ?? _defaultLocalExpiration,
            Flags = flags
        }, tags, cancellationToken);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _hybridCache.RemoveAsync(key, cancellationToken);
    }

    public async ValueTask RemoveByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();

        foreach (var tag in tags)
            tasks.Add(_hybridCache.RemoveByTagAsync(tag, cancellationToken).AsTask());

        await Task.WhenAll(tasks);
    }
}
