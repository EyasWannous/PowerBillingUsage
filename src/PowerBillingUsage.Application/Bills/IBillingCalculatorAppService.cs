using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Bills;

public interface IBillingCalculatorAppService
{
    public Task<Result<BillDto>> CalculateBillAsync(CalculateBillDto input, CancellationToken cancellationToken = default);
    Task<Result<BillReadModelDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<BillReadModelDto>>> GetListAsync(GetBillsDto input, CancellationToken cancellationToken = default);
}
