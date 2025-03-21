using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.EntityFramework.Repository;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;
using PowerBillingUsage.Application.Bills;

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

builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IBillDetailRepository, BillDetailRepository>();
builder.Services.AddScoped<ITierRepository, TierRepository>();
builder.Services.AddScoped<IBillingCalculatorAppService, BillingCalculatorAppService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


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

app.UseAuthorization();

app.MapControllers();

app.Run();
