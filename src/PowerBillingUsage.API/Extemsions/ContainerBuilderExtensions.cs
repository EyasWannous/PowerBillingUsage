using Autofac;
using Autofac.Core;
using System.Reflection;

namespace PowerBillingUsage.API.Extemsions;

public static class ContainerBuilderExtensions
{
    public static void RegisterAutoFacModules(this ContainerBuilder builder)
    {
        const string AssembliesFetchPattern = "PowerBillingUsage.*.dll";

        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrWhiteSpace(path))
            return;

        Directory
            .GetFiles(path, AssembliesFetchPattern, SearchOption.TopDirectoryOnly)
            .Select(Assembly.LoadFrom)
            .ToList()
            .ForEach(assembly =>
            {
                assembly.GetTypes()
                    .Where(p => typeof(IModule).IsAssignableFrom(p) && !p.IsAbstract)
                    .Select(p => Activator.CreateInstance(p) as IModule)
                    .ToList()
                    .ForEach(module => builder.RegisterModule(module!));
            }
        );
    }
}
