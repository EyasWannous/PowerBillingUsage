using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Application;

public static class Mapper
{
    public static BillDto MapBill(this Bill bill)
    {
        return new BillDto
        {
            BillingTypeValue = bill.BillingTypeValue,
            EndAt = bill.EndAt,
            StartAt = bill.StartAt,
            BreakDowns = [.. bill.BreakDowns.Select(MapBillDeatil)],
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

    public static BillReadModelDto MapBillReadModel(this BillReadModel billReadModel)
    {
        return new BillReadModelDto
        {
            Id = billReadModel.Id.Value,
            BillingTypeValue = billReadModel.BillingTypeValue,
            StartAt = billReadModel.StartAt,
            EndAt = billReadModel.EndAt,
            BreakDowns = [.. billReadModel.BreakDowns.Select(MapBillDetailReadModel)],
            Total = billReadModel.Total,
        };
    }

    public static BillDetailReadModelDto MapBillDetailReadModel(this BillDetailReadModel billDetailReadModel)
    {
        return new BillDetailReadModelDto
        {
            TierName = billDetailReadModel.TierName,
            Consumption = billDetailReadModel.Consumption,
            Rate = billDetailReadModel.Rate,
            Total = billDetailReadModel.Total
        };
    }
}
