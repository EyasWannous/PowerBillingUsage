using Autofac;
using PowerBillingUsage.Domain;
using PowerBillingUsage.Domain.Abstractions;
using PowerBillingUsage.Infrastructure.EntityFramework.Repository;
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
    }
}

