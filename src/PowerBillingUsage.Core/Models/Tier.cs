namespace PowerBillingUsage.Core.Models;

public class Tier
{
    public string Name { get; set; } = string.Empty;
    public int UpperLimitInKWh { get; set; }
    public decimal Rate { get; set; }
}
