using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework;

public class PowerBillingUsageWriteDbContext(DbContextOptions<PowerBillingUsageWriteDbContext> options) : DbContext(options)
{
    public DbSet<Bill> Bills { get; set; }
    public DbSet<BillDetail> BillDetails { get; set; }
    public DbSet<Tier> Tiers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PowerBillingUsageWriteDbContext).Assembly,
            WriteConfigurationFillter
        );
    }

    private static bool WriteConfigurationFillter(Type type)
        => type.FullName?.Contains("EntityConfigurations.Write") ?? false;
}
