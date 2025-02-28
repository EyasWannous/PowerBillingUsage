using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Infrastructure.EntityFramework.Repository;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Core.ApplicationContracts;
using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Core.AppServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PowerBillingUsageDbContext>(options =>
    options.UseInMemoryDatabase("PowerBillingUsageDb")
);

builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBillDetailRepository, BillDetailRepository>();
builder.Services.AddScoped<ITierRepository, TierRepository>();
builder.Services.AddScoped<IBillingCalculatorAppService, BillingCalculatorAppService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7216")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowBlazor");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
