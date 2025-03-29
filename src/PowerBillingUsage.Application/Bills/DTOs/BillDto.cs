namespace PowerBillingUsage.Application.Bills.DTOs;

public record BillDto
{
    public int BillingTypeValue { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<BillDetailDto> BreakDowns { get; set; } = [];
    public decimal Total { get; set; }
}
