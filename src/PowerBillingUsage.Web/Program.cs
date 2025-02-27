using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repository;
using PowerBillingUsage.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddDbContext<PowerBillingUsageDbContext>(options =>
    options.UseInMemoryDatabase("PowerBillingUsageDb")
);

builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBillDetailRepository, BillDetailRepository>();
builder.Services.AddScoped<ITierRepository, TierRepository>();

await builder.Build().RunAsync();
