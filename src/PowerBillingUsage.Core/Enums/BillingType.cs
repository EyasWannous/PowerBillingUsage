namespace PowerBillingUsage.Core.Enums;

//public enum BillingType
//{
//    Residential,
//    Commercial
//}

public class BillingType(int value, string name) : SmartEnum<BillingType, int>(value, name)
{
    public static readonly BillingType Residential = new(1, "Residential");
    public static readonly BillingType Commercial = new(2, "Commercial");
}