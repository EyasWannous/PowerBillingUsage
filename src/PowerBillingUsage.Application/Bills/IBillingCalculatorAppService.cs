using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Bills;

public interface IBillingCalculatorAppService
{
    public Task<Result<BillDto>> CalculateBillAsync(CalculateBillDto input, CancellationToken cancellationToken = default);
}
