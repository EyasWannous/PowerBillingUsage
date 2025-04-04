using Autofac;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PowerBillingUsage.API;
using PowerBillingUsage.API.Bills;
using PowerBillingUsage.API.Extemsions;
using PowerBillingUsage.Infrastructure.EntityFramework;
using PowerBillingUsage.Infrastructure.Health;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Host=localhost;Port=xxxxx;Username=xxxxxxxx;Password=xxxxxxxxxxxxxxxxxxx;Database=postgresdb
Console.WriteLine(builder.Configuration.GetConnectionString("postgresdb"));

builder.Services.AddDbContext<PowerBillingUsageWriteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgresdb"))
);

builder.Services.AddDbContext<PowerBillingUsageReadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgresdb"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

//builder.AddRedisDistributedCache("garnetcache");
builder.AddRedisDistributedCache("rediscache");
builder.Services.AddHybridCache();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(builder => builder.RegisterAutoFacModules());

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
);

builder.Services.RegisterRateLimiters();

builder.Services.RegisterCors();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("custom-sql", HealthStatus.Unhealthy)
    .AddNpgSql(builder.Configuration.GetConnectionString("postgresdb")!)
    .AddRedis(builder.Configuration.GetConnectionString("rediscache")!)
    .AddDbContextCheck<PowerBillingUsageWriteDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseRateLimiter();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseExceptionHandler("/Error");
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.MapHealthChecks("health/check", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthorization();

app.MapControllers();

app.MapGroup("/api/v1/bills")
   .WithTags("Bills EndPoints")
   .MapBillEndPoints();

app.UseExceptionHandler();

app.Run();
