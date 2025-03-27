using PowerBillingUsage.Domain;
using System.Reflection;

namespace PowerBillingUsage.Application;

public class ApplicationjModule : AssemblyScanModule
{
    protected override Assembly Assembly => Assembly.GetExecutingAssembly();
}
