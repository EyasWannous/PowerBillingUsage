using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Bills.Commands;

public static class BillErrors
{
    public static Error CalculationFailure(string? message = null) => Error.Failure(
        "Bills.CalculationFailure",
        $"Something Happen that make the bill calculation failed"
        + (message is not null ? $", and an error occurs with this message: {message}" : string.Empty)
    );
}