using PowerBillingUsage.Core.Enums;

namespace PowerBillingUsage.Core.Models;

public class Bill : IEntity<BillId>
{
    public BillId Id { get; private set; }
    //public BillingType BillingType { get; private set; } = BillingType.Residential;
    public int BillingTypeValue { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public List<BillDetail> BreakDowns { get; private set; } = [];

    private Bill() { }

    public Bill(BillId id, int billingTypeValue, DateTime startAt, DateTime endAt, List<BillDetail> breakDowns)
    {
        Id = id;
        BillingTypeValue = billingTypeValue;
        StartAt = startAt;
        EndAt = endAt;
        BreakDowns = breakDowns;
    }

    public decimal Total => Math.Round(BreakDowns.Sum(x => x.Total), 2);
}
