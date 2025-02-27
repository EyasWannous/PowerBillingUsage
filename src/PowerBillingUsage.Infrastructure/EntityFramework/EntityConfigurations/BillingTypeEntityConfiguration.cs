//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using PowerBillingUsage.Core.Enums;

//namespace PowerBillingUsage.Infrastructure.EntityFramework.EntityConfigurations;

//internal class BillingTypeEntityConfiguration : IEntityTypeConfiguration<BillingType>
//{
//    public void Configure(EntityTypeBuilder<BillingType> builder)
//    {
//        builder.HasKey(x => x.Value);
        
//        builder.HasMany(x => x.Tiers);

//        builder.HasData(
//            BillingType.Residential,
//            BillingType.Commercial
//        );
//    }
//}
