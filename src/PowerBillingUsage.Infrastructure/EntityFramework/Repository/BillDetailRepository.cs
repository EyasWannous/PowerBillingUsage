using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Infrastructure.EntityFramework.Repository;

public class BillDetailRepository(PowerBillingUsageDbContext context) 
    : BaseRepository<BillDetail, BillDetailId>(context), IBillDetailRepository
{
}
