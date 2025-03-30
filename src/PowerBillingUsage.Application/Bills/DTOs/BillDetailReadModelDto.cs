﻿namespace PowerBillingUsage.Application.Bills.DTOs;

public record BillDetailReadModelDto
{
    public string TierName { get; set; } = string.Empty;
    public int Consumption { get; set; }
    public decimal Rate { get; set; }
    public decimal Total { get; set; }
}