using PowerBillingUsage.Domain;
using System.Reflection;

namespace PowerBillingUsage.API;

public class ApiModule : AssemblyScanModule
{
    protected override Assembly Assembly => Assembly.GetExecutingAssembly();
}
