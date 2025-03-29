﻿using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
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
            .Build();

        builder.RegisterMediatR(configuration);
    }
}
