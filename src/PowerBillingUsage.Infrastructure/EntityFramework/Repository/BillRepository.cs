using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BillRepository(PowerBillingUsageDbContext context) 
    : BaseRepository<Bill, BillId>(context), IBillRepository
{
}
