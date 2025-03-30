using Microsoft.AspNetCore.Mvc;
using PowerBillingUsage.Application.Bills;
using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Application.DTOs;

namespace PowerBillingUsage.API.Bills;

public static class BillEndPoints
{
    public static IEndpointRouteBuilder MapBillEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("calculate", async(
            [FromBody] CalculateBillDto input,
            [FromServices] IBillingCalculatorAppService billingCalculatorAppService,
            CancellationToken cancellationToken = default) =>
        {
            var response = await billingCalculatorAppService.CalculateBillAsync(input, cancellationToken);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Error);

            return Results.Ok(response.Value);
        })
        .WithOpenApi();

        app.MapGet("/{id:Guid}", async(
            [FromRoute] Guid id,
            [FromServices] IBillingCalculatorAppService billingCalculatorAppService,
            CancellationToken cancellationToken = default) =>
        {
            var response = await billingCalculatorAppService.GetByIdAsync(id, cancellationToken);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Error);

            return Results.Ok(response.Value);
        })
        .WithOpenApi();


        app.MapPost("getbills", async (
            [FromBody] GetPaginateListDto input,
            [FromServices] IBillingCalculatorAppService billingCalculatorAppService,
            CancellationToken cancellationToken = default) =>
        {
            var response = await billingCalculatorAppService.GetListAsync(input, cancellationToken);

            if (!response.IsSuccess)
                return Results.BadRequest(response.Error);

            return Results.Ok(response.Value);
        })
        .WithOpenApi();

        return app;
    }
}
