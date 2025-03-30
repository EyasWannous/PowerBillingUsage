using PowerBillingUsage.Application.Abstractions.Messaging;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Shared;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Application.Bills.Queries.GetBillsQueries;

public record GetPaginateBillsQuery(int Skip, int Take) : IQuery<List<BillReadModel>>;

internal sealed class GetPaginateBillsQueryHandler : IQueryHandler<GetPaginateBillsQuery, List<BillReadModel>>
{
    private readonly IReadRepository<BillReadModel, BillId> _billReadModelRepository;

    public GetPaginateBillsQueryHandler(IReadRepository<BillReadModel, BillId> billReadModelRepository)
    {
        _billReadModelRepository = billReadModelRepository;
    }

    public async Task<Result<List<BillReadModel>>> Handle(GetPaginateBillsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var billReadModels = await _billReadModelRepository.GetPaginateAsync(request.Skip, request.Take, null, cancellationToken);

            return billReadModels.ToList();
        }
        catch (Exception ex)
        {
            return Result< List<BillReadModel>>.ValidationFailure(BillReadModelErrors.GetPaginateBillsFailure(ex.Message));
        }
    }
}
