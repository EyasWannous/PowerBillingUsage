using PowerBillingUsage.Application.Bills;
using PowerBillingUsage.Application.Bills.DTOs;

namespace PowerBillingUsage.API.Bills;

public static class BillEndPoints
{
    public static IEndpointRouteBuilder MapBillEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(string.Empty, async (CalculateBillDto input, IBillingCalculatorAppService billingCalculatorAppService, CancellationToken cancellationToken = default) =>
        {
            var response = await billingCalculatorAppService.CalculateBillAsync(input, cancellationToken);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Error);

            return Results.Ok(response.Value);
        })
        .WithName("Calculate")
        .WithOpenApi();

        return app;
    }
}
