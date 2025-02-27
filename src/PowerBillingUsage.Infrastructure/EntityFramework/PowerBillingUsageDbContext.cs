﻿using Microsoft.EntityFrameworkCore;
using PowerBillingUsage.Core.Enums;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Infrastructure.EntityFramework;

public class PowerBillingUsageDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Bill> Bills { get; set; }
    public DbSet<BillDetail> BillDetails { get; set; }
    public DbSet<Tier> Tiers { get; set; }
    //public DbSet<BillingType> BillingTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PowerBillingUsageDbContext).Assembly);
    }
}
