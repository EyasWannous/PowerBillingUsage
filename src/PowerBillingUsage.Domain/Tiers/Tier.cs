using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Attributes;

namespace PowerBillingUsage.Domain.Tiers;

[CacheEntity(entityType: typeof(Tier))]
public class Tier : IEntity<TierId>
{
    public TierId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int UpperLimitInKWh { get; private set; }
    public decimal Rate { get; private set; }
    public int BillingTypeValue { get; private set; }

    private Tier() { }

    public Tier(TierId id, string name, int upperLimitInKWh, decimal rate, int billingTypeValue)
    {
        Id = id;
        Name = name;
        UpperLimitInKWh = upperLimitInKWh;
        Rate = rate;
        BillingTypeValue = billingTypeValue;
    }
}
