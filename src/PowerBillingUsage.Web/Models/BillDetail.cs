namespace PowerBillingUsage.Web.Models;

public class BillDetail
{
    public BillDetailId Id { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int Consumption { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
}
