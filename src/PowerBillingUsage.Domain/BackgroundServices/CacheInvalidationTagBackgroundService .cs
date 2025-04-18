using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace PowerBillingUsage.Domain.BackgroundServices;

public class CacheInvalidationTagBackgroundService : BackgroundService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly HybridCache _hybridCache;

    public CacheInvalidationTagBackgroundService(
        IConnectionMultiplexer connectionMultiplexer,
        HybridCache hybridCache)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _hybridCache = hybridCache;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connectionMultiplexer.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Literal(PowerBillingUsageDomainConstants.RedisChannelCacheInvalidationTagName),
            async (_, tag) =>
            {
                await _hybridCache.RemoveByTagAsync(tag.ToString(), stoppingToken);
            }
        );

    }
}