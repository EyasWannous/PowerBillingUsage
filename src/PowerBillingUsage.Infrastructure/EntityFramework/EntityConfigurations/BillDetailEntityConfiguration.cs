using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

internal class BillDetailEntityConfiguration : IEntityTypeConfiguration<BillDetail>
{
    public void Configure(EntityTypeBuilder<BillDetail> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(
                billDetailId => billDetailId.Id,
                guid => new BillDetailId(guid)
            )
            .IsRequired();
    }
}
