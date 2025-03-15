namespace PowerBillingUsage.Web.Models;

public class BillCalculationResponse
{
    public int BillingTypeValue { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<BillDetailResponse> BreakDowns { get; set; } = [];
    public decimal Total { get; set; }

    public string ShowType()
    {
        if (BillingTypeValue is 1)
            return "Residential";
        if (BillingTypeValue is 2)
            return "Commercial";

        return "Undefined";
    }
}
