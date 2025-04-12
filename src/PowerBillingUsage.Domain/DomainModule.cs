using Autofac;
using Microsoft.Extensions.Hosting;
using PowerBillingUsage.Domain.BackgroundServices;
using System.Reflection;

namespace PowerBillingUsage.Domain;

public class DomainModule : AssemblyScanModule
{
    protected override Assembly Assembly => Assembly.GetExecutingAssembly();

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        builder.RegisterType<CacheInvalidationKeyBackgroundService>()
           .As<IHostedService>()
           .SingleInstance();

        builder.RegisterType<CacheInvalidationTagBackgroundService>()
           .As<IHostedService>()
           .SingleInstance();
    }
}
