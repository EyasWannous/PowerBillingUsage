using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PowerBillingUsage.API;
using PowerBillingUsage.Application.Bills;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repository;
using PowerBillingUsage.Infrastructure.Health;
using PowerBillingUsage.Infrastructure.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
//builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Host=localhost;Port=xxxxx;Username=xxxxxxxx;Password=xxxxxxxxxxxxxxxxxxx;Database=postgresdb
Console.WriteLine(builder.Configuration.GetConnectionString("postgresdb"));

builder.Services.AddDbContext<PowerBillingUsageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgresdb"))
);

//builder.AddRedisDistributedCache("garnetcache");
builder.AddRedisDistributedCache("rediscache");

builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBillDetailRepository, BillDetailRepository>();
builder.Services.AddScoped<ITierRepository, TierRepository>();
builder.Services.AddScoped<IBillingCalculatorAppService, BillingCalculatorAppService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("FixedWindow", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Path,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2,
            }
        )
    );
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("customPolicy", opt =>
    {
        opt.PermitLimit = 4;
        opt.Window = TimeSpan.FromSeconds(12);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("custom-sql", HealthStatus.Unhealthy)
    .AddNpgSql(builder.Configuration.GetConnectionString("postgresdb")!)
    .AddRedis(builder.Configuration.GetConnectionString("rediscache")!)
    .AddDbContextCheck<PowerBillingUsageDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseRateLimiter();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.MapHealthChecks("health/check", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.Run();
