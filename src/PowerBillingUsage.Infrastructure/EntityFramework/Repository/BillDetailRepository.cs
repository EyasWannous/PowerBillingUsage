using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BillDetailRepository(PowerBillingUsageDbContext context, ICacheService cacheService)
    : BaseRepository<BillDetail, BillDetailId>(context, cacheService), IBillDetailRepository
{
}
