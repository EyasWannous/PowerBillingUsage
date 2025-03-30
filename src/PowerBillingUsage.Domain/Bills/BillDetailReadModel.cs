using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Attributes;

namespace PowerBillingUsage.Domain.Bills;

[RelatedToEntity(typeof(BillDetail))]
public class BillDetailReadModel : IEntity<BillDetailId>
{
    public BillDetailId Id { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int Consumption { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
}
