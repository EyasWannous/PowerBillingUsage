using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations.Write;

internal class BillEntityConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                billId => billId.Value,
                guid => new BillId(guid),
                ValueComparers.GetValueComparer<BillId>()
            )
            .IsRequired();

        builder.Property(x => x.BillingTypeValue).IsRequired();
    }
}
