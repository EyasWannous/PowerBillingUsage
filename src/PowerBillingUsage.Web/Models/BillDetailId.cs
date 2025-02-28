namespace PowerBillingUsage.Web.Models;

public class BillDetailId
{
    public Guid Value { get; set; }

    public BillDetailId(Guid value)
    {
        Value = value;
    }
}
