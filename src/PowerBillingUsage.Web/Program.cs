using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PowerBillingUsage.Web;
using PowerBillingUsage.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient 
    {
        BaseAddress = new Uri("https://localhost:8001")
    }
);

builder.Services.AddScoped<BillingService>();

var app = builder.Build();

await app.RunAsync();