using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Application;
using PowerBillingUsage.Application.Bills;
using PowerBillingUsage.Application.Bills.DTOs;
using PowerBillingUsage.Domain.Abstractions.Services;
using PowerBillingUsage.Infrastructure.EntityFramework;

namespace PowerBillingUsage.API.Bills;

[ApiController]
[Route("api/[Controller]")]
public class BillController : ControllerBase
{
    private readonly IBillingCalculatorAppService _billingCalculatorAppService;
    protected IServiceProvider ServiceProvider;

    public BillController(IBillingCalculatorAppService billingCalculatorAppService, IServiceProvider serviceProvider)
    {
        _billingCalculatorAppService = billingCalculatorAppService;
        ServiceProvider = serviceProvider;
    }

    [HttpPost]
    public async Task<IActionResult> CalculateBill(CalculateBillDto input, CancellationToken cancellationToken = default)
    {
        var response = await _billingCalculatorAppService.CalculateBillAsync(input, cancellationToken);

        if (!response.IsSuccess)
            return BadRequest(response.Error);

        return Ok(response.Value.MapBill());
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        var context = ServiceProvider.GetRequiredService<PowerBillingUsageWriteDbContext>();

        return Ok(await context.Bills.Include(x => x.BreakDowns).ToListAsync());
    }

    [HttpGet("cache")]
    public async Task<IActionResult> TestCache()
    {
        var cacheService = ServiceProvider.GetRequiredService<ICacheService>();

        await cacheService.SetAsync(nameof(TestCache), 10);

        return Ok();
    }
}
