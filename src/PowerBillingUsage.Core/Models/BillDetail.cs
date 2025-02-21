namespace PowerBillingUsage.Core.Models;

public record BillDetail(string TierName, int Consumption, decimal Rate, decimal Total);
