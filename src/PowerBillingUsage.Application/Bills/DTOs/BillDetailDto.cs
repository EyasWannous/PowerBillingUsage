namespace PowerBillingUsage.Application.Bills.DTOs;

public record BillDetailDto
{
    public string TierName { get; set; } = string.Empty;
    public int Consumption { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
}
