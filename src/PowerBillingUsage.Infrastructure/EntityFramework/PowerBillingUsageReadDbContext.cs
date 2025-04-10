﻿using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework;

public class PowerBillingUsageReadDbContext(DbContextOptions<PowerBillingUsageReadDbContext> options) : DbContext(options)
{
    public DbSet<BillReadModel> Bills { get; set; }
    public DbSet<BillDetailReadModel> BillDetails { get; set; }
    public DbSet<TierReadModel> Tiers { get; set; }

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

