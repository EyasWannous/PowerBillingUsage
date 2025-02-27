using PowerBillingUsage.Core.IRepository;
using PowerBillingUsage.Core.Models;

namespace PowerBillingUsage.Core.Extensions;

internal static class InsertToDatabase
{
    internal static Task InsertBillAsync(this Bill bill, IBillRepository billRepository)
    {
        return billRepository.InsertAsync(bill);
    }

    internal static Task InsertTierAsync(this Tier tier, ITierRepository tierRepository)
    {
        return tierRepository.InsertAsync(tier);
    }

    internal static Task InsertBillDetailAsync(this BillDetail billDetail, IBillDetailRepository billDetailRepository)
    {
        return billDetailRepository.InsertAsync(billDetail);
    }
}
