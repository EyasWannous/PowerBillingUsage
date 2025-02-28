using PowerBillingUsage.Web.Models;
using PowerBillingUsage.Web.Requests;
using System.Net.Http.Json;

namespace PowerBillingUsage.Web.Services;

public class BillingService
{
    private readonly HttpClient _httpClient;

    public BillingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BillCalculationResponse?> CalculateBillAsync(BillingRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/bill", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BillCalculationResponse>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error calling API: {ex.Message}");
            return null;
        }
    }
}