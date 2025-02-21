using PowerBillingUsage.Core.Configurations;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.Enums;

//public enum BillingType { Residential, Commercial }

public class BillingType : SmartEnum<BillingType, int>
{
    public static readonly BillingType Residential = new(1, nameof(Residential), BillingConfiguration.ResidentialTiers);
    public static readonly BillingType Commercial = new(2, nameof(Commercial), BillingConfiguration.CommercialTiers);

    private BillingType(int value, string name, List<Tier> tiers) 
        : base(value, name)
    {
        Tiers = tiers;
    }

    public List<Tier> Tiers { get; } = [];
}