using PowerBillingUsage.Core.Enums;

namespace PowerBillingUsage.Core.Models;

public record Bill(BillingType BillingType, DateOnly StartAt, DateOnly EndAt, List<BillDetail> BreakDowns)
{
    public decimal Total => Math.Round(BreakDowns.Sum(x => x.Total), 2);
}
