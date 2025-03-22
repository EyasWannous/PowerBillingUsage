using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class TierRepository(PowerBillingUsageDbContext context, ICacheService cacheService)
    : BaseRepository<Tier, TierId>(context, cacheService), ITierRepository
{
}
