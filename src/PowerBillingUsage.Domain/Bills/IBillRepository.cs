using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Domain.Bills;

public interface IBillRepository : IBaseRepository<Bill, BillId>
{
}
