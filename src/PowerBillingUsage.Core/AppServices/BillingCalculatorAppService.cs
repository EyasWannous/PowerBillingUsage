using PowerBillingUsage.Core.ApplicationContracts;
using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Extensions;
using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.AppServices;

public class BillingCalculatorAppService : IBillingCalculatorAppService
{
    private readonly IBillRepository _billRepository;

    public BillingCalculatorAppService(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<Bill> CalculateCommercialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date.", nameof(startAt));

        return await ExecuteAndSaveAsync(async () =>
        {
            if (consumptionInKWh is 0)
                return new Bill(new BillId(Guid.NewGuid()), BillingType.Commercial.Value, startAt, endAt, []);

            var breakDowns = await CalculateBreakdownsAsync(consumptionInKWh, BillingType.Commercial.Tiers);
            return new Bill(new BillId(Guid.NewGuid()), BillingType.Commercial.Value, startAt, endAt, breakDowns);
        });
    }

    public async Task<Bill> CalculateResidentialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date.", nameof(startAt));

        return await ExecuteAndSaveAsync(async () =>
        {
            if (consumptionInKWh is 0)
                return new Bill(new BillId(Guid.NewGuid()), BillingType.Residential.Value, startAt, endAt, []);

            var breakDowns = await CalculateBreakdownsAsync(consumptionInKWh, BillingType.Residential.Tiers);
            return new Bill(new BillId(Guid.NewGuid()), BillingType.Residential.Value, startAt, endAt, breakDowns);
        });
    }

    private static Task<List<BillDetail>> CalculateBreakdownsAsync(int consumptionInKWh, List<Tier> tiers)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(consumptionInKWh, nameof(consumptionInKWh));

        if (consumptionInKWh is 0)
            return Task.FromResult<List<BillDetail>>([]);

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

        return Task.FromResult(breakDowns);
    }

    private async Task<Bill> ExecuteAndSaveAsync(Func<Task<Bill>> calculation)
    {
        var bill = await calculation();

        await bill.InsertBillAsync(_billRepository);

        return bill;
    }
}
