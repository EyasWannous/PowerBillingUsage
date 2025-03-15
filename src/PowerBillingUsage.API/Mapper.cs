﻿using PowerBillingUsage.API.DTOs;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.API;

public static class Mapper
{
    public static BillDto MapBill(this Bill bill)
    {
        return new BillDto
        {
            BillingTypeValue = bill.BillingTypeValue,
            EndAt = bill.EndAt,
            StartAt = bill.StartAt,
            BreakDowns = bill.BreakDowns.Select(MapBillDeatil).ToList(),
            Total = bill.Total,
        };
    }

    public static BillDetailDto MapBillDeatil(this BillDetail billDetail)
    {
        return new BillDetailDto
        {
            TierName = billDetail.TierName,
            Consumption = billDetail.Consumption,
            Rate = billDetail.Rate,
            Total = billDetail.Total,
        };
    }
}
