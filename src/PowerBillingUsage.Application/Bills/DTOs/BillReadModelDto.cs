namespace PowerBillingUsage.Application.Bills.DTOs;

public record BillReadModelDto
{
    public Guid Id { get; set; }
    public int BillingTypeValue { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<BillDetailReadModelDto> BreakDowns { get; set; } = [];
    public decimal Total { get; set; }
}

