using PowerBillingUsage.Application.Abstractions.Messaging;
using PowerBillingUsage.Domain.Abstractions.Shared;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Enums;

namespace PowerBillingUsage.Application.Bills.Commands.CalculateCommands;

public record CalculateBillCommand(int ConsumptionInKWh, DateTime StartAt, DateTime EndAt, BillingType Type, TimeSpan? Expiration = null) : ICommand<Bill>;

internal sealed class CalculateBillCommandHandler : ICommandHandler<CalculateBillCommand, Bill>
{
    private readonly BillManager _billManager;

    public CalculateBillCommandHandler(BillManager billManager)
    {
        _billManager = billManager;
    }

    public async Task<Result<Bill>> Handle(CalculateBillCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.Type == BillingType.Residential)
            {
                var residentialBill = await _billManager.CalculateResidentialBillAsync(
                    command.ConsumptionInKWh,
                    command.StartAt,
                    command.EndAt,
                    command.Expiration,
                    cancellationToken
                );

                return Result.Success(residentialBill);
            }

            var commercialBill = await _billManager.CalculateCommercialBillAsync(
                command.ConsumptionInKWh,
                command.StartAt,
                command.EndAt,
                command.Expiration,
                cancellationToken
            );

            return commercialBill;
        }
        catch (Exception ex)
        {
            return Result<Bill>.ValidationFailure(BillErrors.CalculationFailure(ex.Message));
        }
    }
}
