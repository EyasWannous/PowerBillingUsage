using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using PowerBillingUsage.Domain.Enums;
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
            RedisChannel.Literal(ConstantNames.RedisChannelCacheInvalidationTagName),
            async (_, tag) =>
            {
                await _hybridCache.RemoveByTagAsync(tag.ToString(), stoppingToken);
            }
        );

    }
}