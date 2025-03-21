using Microsoft.AspNetCore.Components;
using PowerBillingUsage.Web.Models;
using PowerBillingUsage.Web.Requests;
using PowerBillingUsage.Web.Services;

namespace PowerBillingUsage.Web.Pages;

public partial class Home
{
    [Inject]
    private BillingService? _billingService { get; set; } = default;

    private BillingRequest billingRequest = new();
    private BillCalculationResponse? bill = default;
    private bool isLoading = false;

    private async Task GenerateBillAsync()
    {
        isLoading = true;
        try
        {
            if (_billingService is null)
                throw new ArgumentNullException(nameof(_billingService));

            bill = await _billingService.CalculateBillAsync(billingRequest);
        }
        catch (Exception)
        {
            bill = null;
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}