using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

internal class BillDetailEntityConfiguration : IEntityTypeConfiguration<BillDetail>
{
    public void Configure(EntityTypeBuilder<BillDetail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                billDetailId => billDetailId.Value,
                guid => new BillDetailId(guid)
            )
            .IsRequired();
    }
}
