using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Attributes;

namespace PowerBillingUsage.Domain.Bills;

[RelatedToEntity(typeof(Bill))]
public class BillReadModel : IReadModel<BillId>
{
    public BillId Id { get; set; }
    public int BillingTypeValue { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<BillDetailReadModel> BreakDowns { get; set; } = [];
    public decimal Total => Math.Round(BreakDowns.Sum(x => x.Total), 2);
}
