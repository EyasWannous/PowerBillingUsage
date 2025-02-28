namespace PowerBillingUsage.Web.Models;

public class BillId
{
    public Guid Value { get; set; }

    public BillId(Guid value)
    {
        Value = value;
    }
}
