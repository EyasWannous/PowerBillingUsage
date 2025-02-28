using System.ComponentModel.DataAnnotations;

namespace PowerBillingUsage.Web.Requests;

public class BillingRequest
{
    [Range(0, int.MaxValue, ErrorMessage = "Consumption must be a non-negative value.")]
    public int Consumption { get; set; }
    public DateTime StartAt { get; set; } = DateTime.Now;
    public DateTime EndAt { get; set; } = DateTime.Now.AddMonths(1);

    [Range(1, 2)]
    public int BillingTypeValue { get; set; } = 1;
}
