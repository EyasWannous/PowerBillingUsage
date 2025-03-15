using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Application.Bills;

public interface IBillingCalculatorAppService
{
    Task<Bill> CalculateResidentialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt);
    Task<Bill> CalculateCommercialBillAsync(int consumptionInKWh, DateTime startAt, DateTime endAt);
}
