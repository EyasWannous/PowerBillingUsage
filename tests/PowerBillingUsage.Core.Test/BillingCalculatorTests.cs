using FluentAssertions;
using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Models;
using PowerBillingUsage.Core.Services;

namespace PowerBillingUsage.Core.Test;

public class BillingCalculatorTests
{
    private static readonly DateTime DefualtStartDate = new(2025, 01, 01, 20, 10, 00);
    private static readonly DateTime DefualtEndDate = new(2025, 01, 31, 03, 15, 00);

    public static IEnumerable<object[]> GetValidatedResidentialBillingData =>
    [
        [
            0,
            new Bill(
                BillingType: BillingType.Residential,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: []
            )
        ],
        [
            50,
            new Bill(
                BillingType: BillingType.Residential,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 160 KWh", 50, 0.05m, 2.50m)
                ]
            )
        ],
        [
            160,
            new Bill(
                BillingType: BillingType.Residential,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 160 KWh", 160, 0.05m, 8.00m)
                ]
            )
        ],
        [
            250,
            new Bill(
                BillingType: BillingType.Residential,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail("Up to 300 KWh", 90, 0.10m, 9.00m),
                ]
            )
        ],
        [
            450,
            new Bill(
                BillingType: BillingType.Residential,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail("Up to 300 KWh", 140, 0.10m, 14.00m),
                    new BillDetail("Up to 500 KWh", 150, 0.12m, 18.00m),
                ]
            )
        ],
        [
            850,
            new Bill(
                BillingType: BillingType.Residential,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 160 KWh", 160, 0.05m, 8.00m),
                    new BillDetail("Up to 300 KWh", 140, 0.10m, 14.00m),
                    new BillDetail("Up to 500 KWh", 200, 0.12m, 24.00m),
                    new BillDetail("Up to 600 KWh", 100, 0.16m, 16.00m),
                    new BillDetail("Up to 750 KWh", 150, 0.22m, 33.00m),
                    new BillDetail("Up to 1000 KWh", 100, 0.27m, 27.00m),
                ]
            )
        ]
    ];

    public static IEnumerable<object[]> GetValidatedCommercialBillingData =>
    [
        [
            50,
            new Bill(
                BillingType: BillingType.Commercial,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 200 KWh", 50, 0.08m, 4.00m)
                ]
            )
        ],
        [
            300,
            new Bill(
                BillingType: BillingType.Commercial,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 200 KWh", 200, 0.08m, 16.00m),
                    new BillDetail("Up to 500 KWh", 100, 0.15m, 15.00m)
                ]
            )
        ],
        [
            700,
            new Bill(
                BillingType: BillingType.Commercial,
                StartAt: DefualtStartDate,
                EndAt: DefualtEndDate,
                BreakDowns: [
                    new BillDetail("Up to 200 KWh", 200, 0.08m, 16.00m),
                    new BillDetail("Up to 500 KWh", 300, 0.15m, 45.00m),
                    new BillDetail("Above 500 KWh", 200, 0.25m, 50.00m),
                ]
            )
        ]
    ];

    [Theory]
    [InlineData(-1)]
    public void CalculateResidentialConsumptionCost_ShouldThrowArgumentOutOfRangeExceptionForInvalidConsumption(int consumption)
    {
        Action act = () => BillingCalculator.CalculateResidentialBill(consumption, DefualtStartDate, DefualtEndDate);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1)]
    public void CalculateCommercialConsumptionCost_ShouldThrowArgumentOutOfRangeExceptionForInvalidConsumption(int consumption)
    {
        Action act = () => BillingCalculator.CalculateCommercialBill(consumption, DefualtStartDate, DefualtEndDate);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData("2025-01-31", "2025-01-01")]
    public void CalculateResidentialConsumptionCost_ShouldThrowArgumentExceptionForInvalidDateRange(string startAt, string endAt)
    {
        var startDate = DateTime.Parse(startAt);
        var endDate = DateTime.Parse(endAt);

        Action act = () => BillingCalculator.CalculateResidentialBill(100, startDate, endDate);

        act.Should().Throw<ArgumentException>().WithMessage("The start date must not be after the end date.*");
    }

    [Theory]
    [InlineData("2025-01-31", "2025-01-01")]
    public void CalculateCommercialConsumptionCost_ShouldThrowArgumentExceptionForInvalidDateRange(string startAt, string endAt)
    {
        var startDate = DateTime.Parse(startAt);
        var endDate = DateTime.Parse(endAt);

        Action act = () => BillingCalculator.CalculateCommercialBill(100, startDate, endDate);

        act.Should().Throw<ArgumentException>().WithMessage("The start date must not be after the end date.*");
    }

    [Theory]
    [MemberData(nameof(GetValidatedResidentialBillingData))]
    public void CalculateResidentialConsumptionCost_ShouldReturnExpectedValue(int consumption, Bill expected)
    {
        var result = BillingCalculator.CalculateResidentialBill(consumption, DefualtStartDate, DefualtEndDate);

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(GetValidatedCommercialBillingData))]
    public void CalculateCommercialConsumptionCost_ShouldReturnExpectedValue(int consumption, Bill expected)
    {
        var result = BillingCalculator.CalculateCommercialBill(consumption, DefualtStartDate, DefualtEndDate);

        result.Should().BeEquivalentTo(expected);
    }
}
