using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BillRepository(PowerBillingUsageDbContext context, ICacheService cacheService)
    : BaseRepository<Bill, BillId>(context, cacheService), IBillRepository
{
}
