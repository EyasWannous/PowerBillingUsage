using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using PowerBillingUsage.Application.Behaviors;
using PowerBillingUsage.Domain;
using System.Reflection;

namespace PowerBillingUsage.Application;

public class ApplicationModule : AssemblyScanModule
{
    protected override Assembly Assembly => Assembly.GetExecutingAssembly();

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        var configuration = MediatRConfigurationBuilder
            .Create(Assembly)
            .WithAllOpenGenericHandlerTypesRegistered()
            .WithRegistrationScope(RegistrationScope.Transient)
            .WithCustomPipelineBehavior(typeof(LoggingPipelineBehavior<,>))
            .WithCustomPipelineBehavior(typeof(QueryCachedPipelineBehavior<,>))
            .Build();

        //builder.RegisterGeneric(typeof(LoggingPiplineBehavior<,>))
        //    .As(typeof(IPipelineBehavior<,>))
        //    .InstancePerLifetimeScope();

        builder.RegisterMediatR(configuration);
    }
}
