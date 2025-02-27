using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Core.Configurations;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

internal class TierEntityConfiguration : IEntityTypeConfiguration<Tier>
{
    public void Configure(EntityTypeBuilder<Tier> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(
                tierId => tierId.Id,
                guid => new TierId(guid)
            )
            .IsRequired();

        builder.Property(x => x.BillingTypeValue).IsRequired();

        var allTiers = BillingConfiguration.ResidentialTiers.Concat(BillingConfiguration.CommercialTiers)
            .Select(t => new
            {
                Id = new TierId(t.Id.Id),
                t.Name,
                t.UpperLimitInKWh,
                t.Rate,
                t.BillingTypeValue
            }).ToArray();

        builder.HasData(allTiers);

        //var tierIds = BillingConfiguration.ResidentialTiers
        //    .Concat(BillingConfiguration.CommercialTiers)
        //    .Select(t => t.Id.Id)
        //    .ToList();
     
        //if (tierIds.Any(id => id == Guid.Empty))
        //    throw new InvalidOperationException("One or more Tier IDs are Guid.Empty!");
    }
}
