using MediatR;
using PowerBillingUsage.Application.Bills.Commands;
using PowerBillingUsage.Application.Bills.Commands.CalculateCommands;
using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Application.Bills.Queries.GetBillsQueries;
using PowerBillingUsage.Application.DTOs;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Shared;
using PowerBillingUsage.Domain.Enums;

namespace PowerBillingUsage.Application.Bills;

public class BillingCalculatorAppService : IBillingCalculatorAppService, IScopedDependency
{
    private readonly ISender _sender;

    public BillingCalculatorAppService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Result<BillDto>> CalculateBillAsync(CalculateBillDto input, CancellationToken cancellationToken = default)
    {
        var billingType = BillingType.FromValue(input.BillingTypeValue);
        if (billingType is null)
            return Result<BillDto>.ValidationFailure(BillErrors.CalculationFailure());

        var response = await _sender.Send(
            new CalculateBillCommand(
                input.Consumption,
                input.StartAt,
                input.EndAt,
                billingType,
                null
            ),
            cancellationToken
        );

        if (!response.IsSuccess)
            return Result<BillDto>.ValidationFailure(response.Error);

        return response.Value.MapBill();
    }

    public async Task<Result<PaingationResponse<BillReadModelDto>>> GetListAsync(GetPaginateListDto input, CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetPaginateBillsQuery(
                input.Skip,
                input.Take
            ),
            cancellationToken
        );

        if (!response.IsSuccess)
            return Result<PaingationResponse<BillReadModelDto>>.ValidationFailure(response.Error);

        return new PaingationResponse<BillReadModelDto>(
            response.Value.TotalCount,
            [.. response.Value.Items.Select(x => x.MapBillReadModel())]
        );
    }

    public async Task<Result<BillReadModelDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _sender.Send(
            new GetBillByIdQuery(id),
            cancellationToken
        );

        if (!response.IsSuccess)
            return Result<BillReadModelDto?>.ValidationFailure(response.Error);

        return response.Value.MapBillReadModel();

    }
}
