using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Domain.Bills;

public class BillManager : IScopedDependency
{
    private readonly IRepository<Bill, BillId> _billRepository;

    public BillManager(IRepository<Bill, BillId> billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<Bill> CalculateCommercialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date.", nameof(startAt));

        if (consumptionInKWh is 0)
            return new Bill(
                new BillId(Guid.NewGuid()),
                BillingType.Commercial.Value,
                startAt.ToUniversalTime(),
                endAt.ToUniversalTime(),
                []
            );

        var breakDowns = await CalculateBreakdownsAsync(consumptionInKWh, BillingType.Commercial.Tiers);

        var bill = new Bill(
            new BillId(Guid.NewGuid()),
            BillingType.Commercial.Value,
            startAt.ToUniversalTime(),
            endAt.ToUniversalTime(),
            breakDowns
        );

        await _billRepository.InsertAsync(bill, expiration, cancellationToken);

        await _billRepository.SaveChangesAsync(cancellationToken);

        return bill;
    }

    public async Task<Bill> CalculateResidentialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (startAt > endAt)
            throw new ArgumentException("The start date must not be after the end date.", nameof(startAt));

        if (consumptionInKWh is 0)
            return new Bill(
                new BillId(Guid.NewGuid()),
                BillingType.Residential.Value,
                startAt.ToUniversalTime(),
                endAt.ToUniversalTime(),
                []
            );

        var breakDowns = await CalculateBreakdownsAsync(consumptionInKWh, BillingType.Residential.Tiers);
        var bill = new Bill(
            new BillId(Guid.NewGuid()),
            BillingType.Residential.Value,
            startAt.ToUniversalTime(),
            endAt.ToUniversalTime(),
            breakDowns
        );

        await _billRepository.InsertAsync(bill, expiration, cancellationToken);

        await _billRepository.SaveChangesAsync(cancellationToken);

        return bill;
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
}
