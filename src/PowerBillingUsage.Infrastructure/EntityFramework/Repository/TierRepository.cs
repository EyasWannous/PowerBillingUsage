using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class TierRepository(PowerBillingUsageDbContext context)
    : BaseRepository<Tier, TierId>(context), ITierRepository
{
}
