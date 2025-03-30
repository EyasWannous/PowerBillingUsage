using PowerBillingUsage.Application.Abstractions.Messaging;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Domain.Abstractions.Shared;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Application.Bills.Queries.GetBillsQueries;

public record GetBillByIdQuery(Guid Id) : IQuery<BillReadModel?>;

internal sealed class GetBillByIdQueryHandler : IQueryHandler<GetBillByIdQuery, BillReadModel?>
{
    private readonly IReadRepository<BillReadModel, BillId> _billReadModelRepository;

    public GetBillByIdQueryHandler(IReadRepository<BillReadModel, BillId> billReadModelRepository)
    {
        _billReadModelRepository = billReadModelRepository;
    }

    public async Task<Result<BillReadModel?>> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var billReadModel = await _billReadModelRepository.GetByIdAsync(new BillId(request.Id), null, cancellationToken);

            return billReadModel;
        }
        catch (Exception ex)
        {
            return Result<BillReadModel?>.ValidationFailure(BillReadModelErrors.GetPaginateBillsFailure(ex.Message));
        }
    }
}