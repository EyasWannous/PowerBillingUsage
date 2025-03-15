using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class TierRepository(PowerBillingUsageDbContext context)
    : BaseRepository<Tier, TierId>(context), ITierRepository
{
}
