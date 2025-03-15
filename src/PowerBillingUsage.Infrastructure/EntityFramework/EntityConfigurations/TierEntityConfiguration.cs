using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Tiers;
using PowerBillingUsage.Domain.Tiers.Configurations;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

internal class TierEntityConfiguration : IEntityTypeConfiguration<Tier>
{
    public void Configure(EntityTypeBuilder<Tier> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                tierId => tierId.Id,
                guid => new TierId(guid)
            )
            .IsRequired();

        builder.Property(x => x.BillingTypeValue).IsRequired();

        var allTiers = BillingConfiguration.ResidentialTiers
            .Concat(BillingConfiguration.CommercialTiers)
            .Select(t => new
            {
                Id = new TierId(t.Id.Id),
                t.Name,
                t.UpperLimitInKWh,
                t.Rate,
                t.BillingTypeValue
            }).ToArray();

        builder.HasData(allTiers);
    }
}
