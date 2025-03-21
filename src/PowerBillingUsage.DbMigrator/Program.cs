using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PowerBillingUsage.Infrastructure.EntityFramework;

try
{
    // Setup configuration
    IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    // Setup DI
    var services = ConfigureServices(config);
    var serviceProvider = services.BuildServiceProvider();

    // Get DbContext and run migrations
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PowerBillingUsageDbContext>();

    Console.WriteLine("Applying migrations...");
    await dbContext.Database.MigrateAsync();

    //Console.WriteLine("Running data seed...");
    //await SeedData.InitializeAsync(scope.ServiceProvider);

    Console.WriteLine("Database migration and seeding completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    throw;
}

IServiceCollection ConfigureServices(IConfiguration config)
{
    var services = new ServiceCollection();

    // Add logging
    services.AddLogging(builder => builder.AddConsole());

    services.AddServiceDiscovery();

    // Add DbContext
    services.AddDbContext<PowerBillingUsageDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("postgresdb"))
    );

    //// Add seeder
    //services.AddScoped<SeedData>();

    return services;
}
