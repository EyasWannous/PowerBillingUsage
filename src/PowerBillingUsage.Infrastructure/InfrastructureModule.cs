using Autofac;
using PowerBillingUsage.Domain;
using PowerBillingUsage.Domain.Abstractions.Helpers;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
using PowerBillingUsage.Infrastructure.Helpers;
using System.Reflection;

namespace PowerBillingUsage.Infrastructure;

public class InfrastructureModule : AssemblyScanModule
{
    protected override Assembly Assembly => Assembly.GetExecutingAssembly();
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterGeneric(typeof(Repository<,>))
            .As(typeof(IRepository<,>))
            .InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(ReadRepository<,>))
            .As(typeof(IReadRepository<,>))
            .InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(WriteRepository<,>))
            .As(typeof(IWriteRepository<,>))
            .InstancePerLifetimeScope();

        // Get the assemblies you want to scan (this might vary based on your project structure)
        var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName!.StartsWith("PowerBillingUsage")) // filter to your assemblies
            .ToList();

        builder.RegisterType<CacheInvalidationHelper>()
               .As<ICacheInvalidationHelper>()
               .WithParameter("assembliesToScan", assembliesToScan)
               .SingleInstance();
    }
}

