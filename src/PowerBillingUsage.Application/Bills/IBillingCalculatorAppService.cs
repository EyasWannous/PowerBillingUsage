using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Application.DTOs;
using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Bills;

public interface IBillingCalculatorAppService
{
    public Task<Result<BillDto>> CalculateBillAsync(CalculateBillDto input, CancellationToken cancellationToken = default);
    Task<Result<BillReadModelDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginatedResponse<BillReadModelDto>>> GetListAsync(GetPaginateListDto input, CancellationToken cancellationToken = default);
}
