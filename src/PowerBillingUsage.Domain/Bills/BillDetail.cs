using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Attributes;

namespace PowerBillingUsage.Domain.Bills;

[CacheEntity(entityType: typeof(BillDetail))]
public record BillDetail : IEntity<BillDetailId>
{
    public BillDetailId Id { get; private set; }
    public string TierName { get; private set; } = string.Empty;
    public int Consumption { get; private set; }
    public decimal Rate { get; private set; }
    public decimal Total { get; private set; }

    private BillDetail() { }

    public BillDetail(BillDetailId id, string tierName, int consumption, decimal rate, decimal total)
    {
        Id = id;
        TierName = tierName;
        Consumption = consumption;
        Rate = rate;
        Total = total;
    }
}
