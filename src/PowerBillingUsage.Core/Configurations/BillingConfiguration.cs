using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.Configurations;

public class BillingConfiguration
{
    public static readonly List<Tier> ResidentialTiers =
    [
        new Tier { Name = "Up to 160 KWh", UpperLimitInKWh = 160, Rate = 0.05m },
        new Tier { Name = "Up to 300 KWh", UpperLimitInKWh = 300, Rate = 0.10m },
        new Tier { Name = "Up to 500 KWh", UpperLimitInKWh = 500, Rate = 0.12m },
        new Tier { Name = "Up to 600 KWh", UpperLimitInKWh = 600, Rate = 0.16m },
        new Tier { Name = "Up to 750 KWh", UpperLimitInKWh = 750, Rate = 0.22m },
        new Tier { Name = "Up to 1000 KWh", UpperLimitInKWh = 1000, Rate = 0.27m },
        new Tier { Name = "Above 1000 KWh", UpperLimitInKWh = int.MaxValue, Rate = 0.37m }
    ];

    public static readonly List<Tier> CommercialTiers =
    [
        new Tier { Name = "Up to 200 KWh", UpperLimitInKWh = 200, Rate = 0.08m },
        new Tier { Name = "Up to 500 KWh", UpperLimitInKWh = 500, Rate = 0.15m },
        new Tier { Name = "Above 500 KWh", UpperLimitInKWh = int.MaxValue, Rate = 0.25m }
    ];
}
