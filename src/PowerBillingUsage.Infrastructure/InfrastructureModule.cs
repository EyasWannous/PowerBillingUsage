using Autofac;
using PowerBillingUsage.Domain;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using PowerBillingUsage.Infrastructure.EntityFramework.Repositories;
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
    }
}

