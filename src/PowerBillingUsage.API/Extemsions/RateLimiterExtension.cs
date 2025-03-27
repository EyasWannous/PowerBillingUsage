using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace PowerBillingUsage.API.Extemsions;

public static class RateLimiterExtension
{
    public static IServiceCollection RegisterRateLimiters(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("FixedWindow", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Request.Path,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(10),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2,
                    }
                )
            );
        });

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("customPolicy", opt =>
            {
                opt.PermitLimit = 4;
                opt.Window = TimeSpan.FromSeconds(12);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });
        });

        return services;
    }
}
