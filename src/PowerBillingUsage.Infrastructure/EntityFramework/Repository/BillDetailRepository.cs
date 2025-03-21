using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BillDetailRepository(PowerBillingUsageDbContext context)
    : BaseRepository<BillDetail, BillDetailId>(context), IBillDetailRepository
{
}
