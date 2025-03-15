using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

internal class BillEntityConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                billId => billId.Id,
                guid => new BillId(guid)
            )
            .IsRequired();

        builder.Property(x => x.BillingTypeValue).IsRequired();
    }
}
