﻿using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Attributes;

namespace PowerBillingUsage.Domain.Bills;

[CacheEntity(entityType: typeof(Bill))]
public class Bill : IEntity<BillId>
{
    public BillId Id { get; private set; }
    public int BillingTypeValue { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public List<BillDetail> BreakDowns { get; private set; } = [];

    private Bill() { }

    public Bill(BillId id, int billingTypeValue, DateTime startAt, DateTime endAt, List<BillDetail> breakDowns)
    {
        Id = id;
        BillingTypeValue = billingTypeValue;
        StartAt = startAt;
        EndAt = endAt;
        BreakDowns = breakDowns;
    }

    public decimal Total => Math.Round(BreakDowns.Sum(x => x.Total), 2);
}
