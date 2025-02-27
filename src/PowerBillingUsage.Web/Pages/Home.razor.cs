using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Models;
using PowerBillingUsage.Core.Services;
using PowerBillingUsage.Web.Requests;
using Microsoft.AspNetCore.Components;
using PowerBillingUsage.Core.IRepository;

namespace PowerBillingUsage.Web.Pages;

public partial class Home
{
    [Inject]
    private IBillRepository BillRepository { get; set; } = default!;

    [Inject]
    private ILogger<Home> Logger { get; set; } = default!;

    private BillingRequest billingRequest = new();
    private Bill? bill = default;
    private bool isLoading = false;

    private async Task GenerateBillAsync()
    {
        isLoading = true;
        Logger.LogInformation("Starting bill generation...");
        try
        {
            if (billingRequest.BillingType == BillingType.Residential.Value)
            {
                bill = await BillingCalculator.CalculateResidentialBillAsync(
                    billingRequest.Consumption,
                    billingRequest.StartAt,
                    billingRequest.EndAt,
                    BillRepository
                );
                Logger.LogInformation("Residential bill generated: {Total}", bill?.Total.ToString() ?? "null");
            }
            else
            {
                bill = await BillingCalculator.CalculateCommercialBillAsync(
                    billingRequest.Consumption,
                    billingRequest.StartAt,
                    billingRequest.EndAt,
                    BillRepository
                );
                Logger.LogInformation("Commercial bill generated: {Total}", bill?.Total.ToString() ?? "null");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error generating bill: {Message}", ex.Message);
            bill = null;
        }
        finally
        {
            isLoading = false;
            Logger.LogInformation("Loading complete, bill is: {Status}", bill != null ? "set" : "null");
            StateHasChanged();
        }
    }
}