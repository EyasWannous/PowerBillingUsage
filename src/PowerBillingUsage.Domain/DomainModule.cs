using System.Reflection;

namespace PowerBillingUsage.Domain;

public class DomainModule : AssemblyScanModule
{
    protected override Assembly Assembly => Assembly.GetExecutingAssembly();
}
