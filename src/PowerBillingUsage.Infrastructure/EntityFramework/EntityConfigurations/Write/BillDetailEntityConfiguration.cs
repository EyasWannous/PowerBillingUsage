using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Bills;
using PowerBillingUsage.Domain.Tiers;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations.Write;

internal class BillDetailEntityConfiguration : IEntityTypeConfiguration<BillDetail>
{
    public void Configure(EntityTypeBuilder<BillDetail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                billDetailId => billDetailId.Value,
                guid => new BillDetailId(guid),
                ValueComparers.GetValueComparer<BillDetailId>()
            )
            .IsRequired();
    }
}
