using PowerBillingUsage.Domain.Abstractions.Shared;

namespace PowerBillingUsage.Application.Bills.Queries;

public static class BillReadModelErrors
{
    public static Error GetPaginateBillsFailure(string? message = null) => Error.Failure(
        "Bills.GetPaginateBillsFailure",
        $"Something Happen that make the process of getting bills failed"
        + (message is not null ? $", and an error occurs with this message: {message}" : string.Empty)
    );
}