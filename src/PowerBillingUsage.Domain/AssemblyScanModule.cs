using Autofac;
using PowerBillingUsage.Domain.Abstractions.RegisteringDependencies;
using PowerBillingUsage.Domain.Abstractions.Repositories;
using System.Reflection;

namespace PowerBillingUsage.Domain;

public abstract class AssemblyScanModule : Autofac.Module
{
    protected abstract Assembly Assembly { get; }
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly)
            .Where(t => typeof(ITransientDependency).IsAssignableFrom(t) && !t.IsInterface)
            .AsImplementedInterfaces()
            .AsSelf()
            .InstancePerDependency();

        builder.RegisterAssemblyTypes(Assembly)
            .Where(t => typeof(IScopedDependency).IsAssignableFrom(t) && !t.IsInterface)
            .AsImplementedInterfaces()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly)
            .Where(t => typeof(ISingletonDependency).IsAssignableFrom(t) && !t.IsInterface)
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();

        //builder.RegisterAssemblyTypes(Assembly)
        //    .AsImplementedInterfaces();

        builder.RegisterAssemblyTypes(Assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IRepository<,>)))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(Assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IReadRepository<,>)))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope(); 

        builder.RegisterAssemblyTypes(Assembly)
            .Where(t => t.IsClosedTypeOf(typeof(IWriteRepository<,>)))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }

}
