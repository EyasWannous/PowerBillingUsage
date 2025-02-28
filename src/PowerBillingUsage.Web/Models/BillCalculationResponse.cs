namespace PowerBillingUsage.Web.Models;

public class BillCalculationResponse
{
    public BillId Id { get; set; }
    public int BillingTypeValue { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<BillDetail> BreakDowns { get; set; } = new();
    public decimal Total => Math.Round(BreakDowns.Sum(x => x.Total), 2);
}
