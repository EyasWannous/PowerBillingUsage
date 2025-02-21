using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.Services;

public class BillingCalculator
{
    public static Bill CalculateResidentialBill(int consumptionInKWh, DateTime startAt, DateTime endAt)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date..", nameof(startAt));

        if (consumptionInKWh is 0)
            return new Bill(BillingType.Residential, startAt, endAt, []);

        var breakDowns = CalculateBreakdowns(consumptionInKWh, BillingType.Residential.Tiers);

        return new Bill(BillingType.Residential, startAt, endAt, breakDowns);
    }

    public static Bill CalculateCommercialBill(int consumptionInKWh, DateTime startAt, DateTime endAt)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date..", nameof(startAt));

        if (consumptionInKWh is 0)
            return new Bill(BillingType.Commercial, startAt, endAt, []);

        var breakDowns = CalculateBreakdowns(consumptionInKWh, BillingType.Commercial.Tiers);

        return new Bill(BillingType.Commercial, startAt, endAt, breakDowns);
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
                    TierName: tier.Name,
                    Consumption: tierConsumption,
                    Rate: tier.Rate,
                    Total: tier.Rate * tierConsumption
                )
            );

            previousUpperLimit = tier.UpperLimitInKWh;
            remainingConsumption -= tierConsumption;
        }

        return breakDowns;
    }
}
