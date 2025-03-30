using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations.Read;

internal class TierReadModelEntityConfiguration : IEntityTypeConfiguration<TierReadModel>
{
    public void Configure(EntityTypeBuilder<TierReadModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                tierId => tierId.Value,
                guid => new TierId(guid),
                ValueComparers.GetValueComparer<TierId>()
            )
            .IsRequired();

        builder.Property(x => x.BillingTypeValue).IsRequired();
    }
}
