using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Domain.Bills;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations.Read;

internal class BillDetailReadModelEntityConfiguration : IEntityTypeConfiguration<BillDetailReadModel>
{
    public void Configure(EntityTypeBuilder<BillDetailReadModel> builder)
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
