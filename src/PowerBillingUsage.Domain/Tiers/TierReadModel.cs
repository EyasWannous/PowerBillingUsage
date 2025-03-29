using PowerBillingUsage.Domain.Abstractions;

namespace PowerBillingUsage.Domain.Tiers;

public class TierReadModel : IReadModel<TierId>
{
    public TierId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UpperLimitInKWh { get; set; }
    public decimal Rate { get; set; }
    public int BillingTypeValue { get; set; }
}

