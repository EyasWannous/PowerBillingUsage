using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Extensions;
using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.Services;

public class BillingCalculator
{
    public static async Task<Bill> CalculateResidentialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt, IBillRepository billRepository)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date.", nameof(startAt));

        return await ExecuteAndSaveAsync(() =>
        {
            if (consumptionInKWh == 0)
                return new Bill(new BillId(Guid.NewGuid()), BillingType.Residential.Value, startAt, endAt, []);

            var breakDowns = CalculateBreakdowns(consumptionInKWh, BillingType.Residential.Tiers);
            return new Bill(new BillId(Guid.NewGuid()), BillingType.Residential.Value, startAt, endAt, breakDowns);

        }, billRepository);
    }

    public static async Task<Bill> CalculateCommercialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt, IBillRepository billRepository)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date.", nameof(startAt));

        return await ExecuteAndSaveAsync(() =>
        {
            if (consumptionInKWh == 0)
                return new Bill(new BillId(Guid.NewGuid()), BillingType.Commercial.Value, startAt, endAt, []);

            var breakDowns = CalculateBreakdowns(consumptionInKWh, BillingType.Commercial.Tiers);
            return new Bill(new BillId(Guid.NewGuid()), BillingType.Commercial.Value, startAt, endAt, breakDowns);

        }, billRepository);
    }

    private static List<BillDetail> CalculateBreakdowns(int consumptionInKWh, List<Tier> tiers)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(consumptionInKWh, nameof(consumptionInKWh));

        if (consumptionInKWh is 0)
            return [];

        List<BillDetail> breakDowns = [];

        int previousUpperLimit = 0;
        int remainingConsumption = consumptionInKWh;
        foreach (var tier in tiers)
        {
            int tierCapacity = tier.UpperLimitInKWh - previousUpperLimit;

            int tierConsumption = Math.Min(remainingConsumption, tierCapacity);

            if (tierConsumption <= 0) break;

            breakDowns.Add(
                new BillDetail(
                    id: new BillDetailId(Guid.NewGuid()),
                    tierName: tier.Name,
                    consumption: tierConsumption,
                    rate: tier.Rate,
                    total: tier.Rate * tierConsumption
                )
            );

            previousUpperLimit = tier.UpperLimitInKWh;
            remainingConsumption -= tierConsumption;
        }

        return breakDowns;
    }

    private static async Task<Bill> ExecuteAndSaveAsync(Func<Bill> calculation, IBillRepository billRepository)
    {
        var bill = calculation();

        await bill.InsertBillAsync(billRepository);

        return bill;
    }
}
