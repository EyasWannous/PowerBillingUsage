using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework;

public class PowerBillingUsageReadDbContext(DbContextOptions<PowerBillingUsageReadDbContext> options) : DbContext(options)
{
    public DbSet<BillReadModel> BillReadModels { get; set; }
    public DbSet<BillDetailReadModel> BillDetailReadModels { get; set; }
    public DbSet<TierReadModel> TierReadModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PowerBillingUsageReadDbContext).Assembly,
            ReadConfigurationFillter
        );
    }

    private static bool ReadConfigurationFillter(Type type)
        => type.FullName?.Contains("EntityConfigurations.Read") ?? false;
}

