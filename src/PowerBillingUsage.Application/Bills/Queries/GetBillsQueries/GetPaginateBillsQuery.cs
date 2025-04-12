using PowerBillingUsage.Application.Abstractions.Messaging;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Shared;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Application.Bills.Queries.GetBillsQueries;

public record GetPaginateBillsQuery(int Skip, int Take) : IQuery<PaginatedResponse<BillReadModel>>;

internal sealed class GetPaginateBillsQueryHandler : IQueryHandler<GetPaginateBillsQuery, PaginatedResponse<BillReadModel>>
{
    private readonly IReadRepository<BillReadModel, BillId> _billReadModelRepository;

    public GetPaginateBillsQueryHandler(IReadRepository<BillReadModel, BillId> billReadModelRepository)
    {
        _billReadModelRepository = billReadModelRepository;
    }

    public async Task<Result<PaginatedResponse<BillReadModel>>> Handle(GetPaginateBillsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _billReadModelRepository.GetPaginateAsync(request.Skip, request.Take, null, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResponse<BillReadModel>>.ValidationFailure(BillReadModelErrors.GetPaginateBillsFailure(ex.Message));
        }
    }
}
