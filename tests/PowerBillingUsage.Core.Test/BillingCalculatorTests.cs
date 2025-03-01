using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Core.AppServices;
using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Models;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repository;

namespace PowerBillingUsage.Core.Test;

public class BillingCalculatorTests
{
    private static readonly DateTime DefaultStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefaultEndDate = new(2025, 01, 31, 03, 15, 00);

    private readonly PowerBillingUsageDbContext _context;
    private readonly BillingCalculatorAppService _billingCalculatorAppService;

    public static IEnumerable<object[]> GetValidatedResidentialBillingData =>
    [
        [
            0,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: []
            )
        ],
        [
            50,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 50, 0.05m, 2.50m)
                ]
            )
        ],
        [
            160,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 160, 0.05m, 8.00m)
                ]
            )
        ],
        [
            250,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 300 KWh", 90, 0.10m, 9.00m),
                ]
            )
        ],
        [
            450,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 300 KWh", 140, 0.10m, 14.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 500 KWh", 150, 0.12m, 18.00m),
                ]
            )
        ],
        [
            850,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Residential.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 300 KWh", 140, 0.10m, 14.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 500 KWh", 200, 0.12m, 24.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 600 KWh", 100, 0.16m, 16.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 750 KWh", 150, 0.22m, 33.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 1000 KWh", 100, 0.27m, 27.00m),
                ]
            )
        ]
    ];

    public static IEnumerable<object[]> GetValidatedCommercialBillingData =>
    [
        [
            50,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Commercial.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 200 KWh", 50, 0.08m, 4.00m)
                ]
            )
        ],
        [
            300,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Commercial.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 200 KWh", 200, 0.08m, 16.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 500 KWh", 100, 0.15m, 15.00m)
                ]
            )
        ],
        [
            700,
            new Bill(
                id: new(Guid.NewGuid()),
                billingTypeValue: BillingType.Commercial.Value,
                startAt: DefaultStartDate,
                endAt: DefaultEndDate,
                breakDowns: [
                    new BillDetail(new(Guid.NewGuid()), "Up to 200 KWh", 200, 0.08m, 16.00m),
                    new BillDetail(new(Guid.NewGuid()), "Up to 500 KWh", 300, 0.15m, 45.00m),
                    new BillDetail(new(Guid.NewGuid()), "Above 500 KWh", 200, 0.25m, 50.00m),
                ]
            )
        ]
    ];

    public BillingCalculatorTests()
    {
        var options = new DbContextOptionsBuilder<PowerBillingUsageDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PowerBillingUsageDbContext(options);

        _billingCalculatorAppService = new BillingCalculatorAppService(new BillRepository(_context));
    }

    internal void Dispose()
    {
        _context.Dispose();
    }

    [Theory]
    [InlineData(-1)]
    public async Task CalculateResidentialConsumptionCost_ShouldThrowArgumentOutOfRangeExceptionForInvalidConsumption(int consumption)
    {
        Func<Task> act = () => _billingCalculatorAppService.CalculateResidentialBillAsync(consumption, DefaultStartDate, DefaultEndDate);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1)]
    public async Task CalculateCommercialConsumptionCost_ShouldThrowArgumentOutOfRangeExceptionForInvalidConsumption(int consumption)
    {
        Func<Task> act = () => _billingCalculatorAppService.CalculateCommercialBillAsync(consumption, DefaultStartDate, DefaultEndDate);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("2025-01-31", "2025-01-01")]
    public async Task CalculateResidentialConsumptionCost_ShouldThrowArgumentExceptionForInvalidDateRange(string startAt, string endAt)
    {
        var startDate = DateTime.Parse(startAt);
        var endDate = DateTime.Parse(endAt);

        Func<Task> act = () => _billingCalculatorAppService.CalculateResidentialBillAsync(100, startDate, endDate);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("The start date must not be after the end date.*");
    }

    [Theory]
    [InlineData("2025-01-31", "2025-01-01")]
    public async Task CalculateCommercialConsumptionCost_ShouldThrowArgumentExceptionForInvalidDateRange(string startAt, string endAt)
    {
        var startDate = DateTime.Parse(startAt);
        var endDate = DateTime.Parse(endAt);

        Func<Task> act = () => _billingCalculatorAppService.CalculateCommercialBillAsync(100, startDate, endDate);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("The start date must not be after the end date.*");
    }

    [Theory]
    [MemberData(nameof(GetValidatedResidentialBillingData))]
    public async Task CalculateResidentialConsumptionCost_ShouldReturnExpectedValue(int consumption, Bill expected)
    {
        var result = await _billingCalculatorAppService.CalculateResidentialBillAsync(consumption, DefaultStartDate, DefaultEndDate);

        result.Id.Should().NotBe(expected.Id);
        result.BillingTypeValue.Should().Be(expected.BillingTypeValue);
        result.StartAt.Should().Be(expected.StartAt);
        result.EndAt.Should().Be(expected.EndAt);

        result.BreakDowns.Should().HaveCount(expected.BreakDowns.Count);
        for (int i = 0; i < result.BreakDowns.Count; i++)
        {
            result.BreakDowns[i].Id.Should().NotBe(expected.BreakDowns[i].Id);
            result.BreakDowns[i].TierName.Should().Be(expected.BreakDowns[i].TierName);
            result.BreakDowns[i].Consumption.Should().Be(expected.BreakDowns[i].Consumption);
            result.BreakDowns[i].Rate.Should().Be(expected.BreakDowns[i].Rate);
            result.BreakDowns[i].Total.Should().Be(expected.BreakDowns[i].Total);
        }
    }

    [Theory]
    [MemberData(nameof(GetValidatedCommercialBillingData))]
    public async Task CalculateCommercialConsumptionCost_ShouldReturnExpectedValue(int consumption, Bill expected)
    {
        var result = await _billingCalculatorAppService.CalculateCommercialBillAsync(consumption, DefaultStartDate, DefaultEndDate);

        result.Id.Should().NotBe(expected.Id);
        result.BillingTypeValue.Should().Be(expected.BillingTypeValue);
        result.StartAt.Should().Be(expected.StartAt);
        result.EndAt.Should().Be(expected.EndAt);

        result.BreakDowns.Should().HaveCount(expected.BreakDowns.Count);
        for (int i = 0; i < result.BreakDowns.Count; i++)
        {
            result.BreakDowns[i].Id.Should().NotBe(expected.BreakDowns[i].Id);
            result.BreakDowns[i].TierName.Should().Be(expected.BreakDowns[i].TierName);
            result.BreakDowns[i].Consumption.Should().Be(expected.BreakDowns[i].Consumption);
            result.BreakDowns[i].Rate.Should().Be(expected.BreakDowns[i].Rate);
            result.BreakDowns[i].Total.Should().Be(expected.BreakDowns[i].Total);
        }
    }
}
