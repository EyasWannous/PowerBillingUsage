using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.ApplicationContracts;

public interface IBillingCalculatorAppService
{
    Task<Bill> CalculateResidentialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt);
    Task<Bill> CalculateCommercialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt);
}
