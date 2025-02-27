using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BillRepository(PowerBillingUsageDbContext context) 
    : BaseRepository<Bill, BillId>(context), IBillRepository
{
}
