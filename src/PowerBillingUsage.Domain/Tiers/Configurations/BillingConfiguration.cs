namespace PowerBillingUsage.Domain.Tiers.Configurations;

public class BillingConfiguration
{
    private static readonly Guid tierOneId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid tierTwoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid tierThreeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid tierFourId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid tierFiveId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid tierSixId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid tierSevenId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid tierEightId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid tierNineId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid tierTenId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly List<Tier> ResidentialTiers =
    [
        new Tier(id: new TierId(tierOneId), name: "Up to 160 KWh", upperLimitInKWh: 160, rate: 0.05m, billingTypeValue: 1),
        new Tier(id: new TierId(tierTwoId), name: "Up to 300 KWh", upperLimitInKWh: 300, rate: 0.10m, billingTypeValue: 1),
        new Tier(id: new TierId(tierThreeId), name: "Up to 500 KWh", upperLimitInKWh: 500, rate: 0.12m, billingTypeValue: 1),
        new Tier(id: new TierId(tierFourId), name: "Up to 600 KWh", upperLimitInKWh: 600, rate: 0.16m, billingTypeValue: 1),
        new Tier(id: new TierId(tierFiveId), name: "Up to 750 KWh", upperLimitInKWh: 750, rate: 0.22m, billingTypeValue: 1),
        new Tier(id: new TierId(tierSixId), name: "Up to 1000 KWh", upperLimitInKWh: 1000, rate: 0.27m, billingTypeValue: 1),
        new Tier(id: new TierId(tierSevenId), name: "Above 1000 KWh", upperLimitInKWh: int.MaxValue, rate: 0.37m, billingTypeValue: 1)
    ];

    public static readonly List<Tier> CommercialTiers =
    [
        new Tier(id: new TierId(tierEightId), name: "Up to 200 KWh", upperLimitInKWh: 200, rate: 0.08m, billingTypeValue: 2),
        new Tier(id: new TierId(tierNineId), name: "Up to 500 KWh", upperLimitInKWh: 500, rate: 0.15m, billingTypeValue: 2),
        new Tier(id: new TierId(tierTenId), name: "Above 500 KWh", upperLimitInKWh: int.MaxValue, rate: 0.25m, billingTypeValue: 2)
    ];
}
