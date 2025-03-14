using Microsoft.AspNetCore.Mvc;
using PowerBillingUsage.API.DTOs;
using PowerBillingUsage.Core.ApplicationContracts;
using PowerBillingUsage.Core.Enums;

namespace PowerBillingUsage.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class BillController : ControllerBase
{
    private readonly IBillingCalculatorAppService _billingCalculatorAppService;

    public BillController(IBillingCalculatorAppService billingCalculatorAppService)
    {
        _billingCalculatorAppService = billingCalculatorAppService;
    }

    [HttpPost]
    public async Task<IActionResult> CalculateBill(CalculateBillDto input)
    {
        try
        {
            if (input.BillingTypeValue == BillingType.Residential.Value)
            {
                var residentialBill = await _billingCalculatorAppService.CalculateResidentialBillAsync(
                    input.Consumption,
                    input.StartAt,
                    input.EndAt
                );

                return Ok(residentialBill.MapBill());
            }

            var commercialBill = await _billingCalculatorAppService.CalculateCommercialBillAsync(
                input.Consumption,
                input.StartAt,
                input.EndAt
            );

            return Ok(commercialBill.MapBill());
        }
        catch (Exception ex) 
        {
            return BadRequest(ex.Message);
        }
    }
}
