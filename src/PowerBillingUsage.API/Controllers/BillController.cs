using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.API.DTOs;
using PowerBillingUsage.Application.Bills;
using PowerBillingUsage.Domain.Enums;
using PowerBillingUsage.Infrastructure.EntityFramework;

namespace PowerBillingUsage.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class BillController : ControllerBase
{
    private readonly IBillingCalculatorAppService _billingCalculatorAppService;
    protected IServiceProvider _serviceProvider;

    public BillController(IBillingCalculatorAppService billingCalculatorAppService, IServiceProvider serviceProvider)
    {
        _billingCalculatorAppService = billingCalculatorAppService;
        _serviceProvider = serviceProvider;
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

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        var context = _serviceProvider.GetRequiredService<PowerBillingUsageDbContext>();

        return Ok(await context.Bills.Include(x => x.BreakDowns).ToListAsync());
    }
}
