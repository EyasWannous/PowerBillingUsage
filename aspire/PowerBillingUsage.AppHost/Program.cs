using Aspirant.Hosting;
using Aspire.Hosting.Lifecycle;
using PowerBillingUsage.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddLifecycleHook<LifecycleLogger>();

var powerBillingUsagePostgres = builder.AddPostgres(Constant.Postgres)
    .WithImage("postgres")
    .WithImageTag("latest")
    //.WithPgAdmin()
    //.WithDataVolume("postgres-data")
    .WithLifetime(ContainerLifetime.Session)
    ;

var postgresdb = powerBillingUsagePostgres.AddDatabase(Constant.PostgresDb);
//var postgresAdmindb = powerBillingUsagePostgres.AddDatabase(Constant.AdminPostgresDb);

//var garnet = builder.AddGarnet(Constant.GarnetCache)
//    .WithImage("ghcr.io/microsoft/garnet")
//    .WithImageTag("latest")
//    //.WithArgs("--lua-transaction-mode")
//    .WithEndpoint(name: "garnet-tcp", port: 6379, targetPort: 6379)
//    .WithLifetime(ContainerLifetime.Session)
//    ;

var redis = builder.AddRedis(Constant.RedisCache)
    .WithImage("redis")
    .WithImageTag("latest")
    .WithEndpoint(name: "redis-tcp", port: 6379, targetPort: 6379)
    .WithLifetime(ContainerLifetime.Session)
    ;

var migration = builder.AddProject<Projects.PowerBillingUsage_DbMigrator>(Constant.PowerBillingUsage_DbMigrator)
    .WithReference(postgresdb)
    .WaitFor(postgresdb, WaitBehavior.WaitOnResourceUnavailable)
    ;

var powerBillingUsageApi = builder.AddProject<Projects.PowerBillingUsage_API>(Constant.PowerBillingUsage_API_Name)
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb)
    //.WithReference(postgresAdmindb)
    .WithReference(redis)
    .WaitFor(postgresdb, WaitBehavior.WaitOnResourceUnavailable)
    .WaitFor(redis, WaitBehavior.WaitOnResourceUnavailable)
    .WaitForCompletion(migration)
    ;

var powerBillingUsage_Web = builder.AddProject<Projects.PowerBillingUsage_Web>(Constant.PowerBillingUsage_Web_Name)
    .WithReference(powerBillingUsageApi)
    .WithReference(redis)
    .WaitFor(powerBillingUsageApi, WaitBehavior.WaitOnResourceUnavailable)
    ;

var powerBillingUsageYarp = builder.AddYarp(Constant.Yarp)
    .WithHttpsEndpoint(port: 8001, targetPort: 7046)
    .WithReference(powerBillingUsageApi)
    .LoadFromConfiguration("ReverseProxy")
    ;

var app = builder.Build();

app.Run();