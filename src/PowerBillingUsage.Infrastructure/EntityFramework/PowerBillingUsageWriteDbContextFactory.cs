using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PowerBillingUsage.Infrastructure.EntityFramework;

public class PowerBillingUsageWriteDbContextFactory : IDesignTimeDbContextFactory<PowerBillingUsageWriteDbContext>
{
    public PowerBillingUsageWriteDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

        // Load configuration (appsettings.json)
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        // Try to get the connection string from config
        var connectionString = configuration.GetConnectionString("postgresdb");

        // Fallback connection string for design-time (replace with your local Postgres details)
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Host=localhost;Port=5432;Username=postgres;Password=yourpassword;Database=yourdbname";
        }

        var optionsBuilder = new DbContextOptionsBuilder<PowerBillingUsageWriteDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new PowerBillingUsageWriteDbContext(optionsBuilder.Options);
    }
}
