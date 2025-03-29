using MediatR;
using PowerBillingUsage.Application.Bills.Commands;
using PowerBillingUsage.Application.Bills.DTOs;
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
}
