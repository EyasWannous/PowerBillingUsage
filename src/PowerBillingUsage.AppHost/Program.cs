using Aspirant.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

const string PowerBillingUsage_DbMigrator = "powerbillingusage-dbmigrator";
const string PowerBillingUsage_API_Name = "powerbillingusage-api";
const string PowerBillingUsage_Web_Name = "powerbillingusage-blazor";
const string redisCache = "rediscache";
const string postgres = "postgres";
const string postgresDb = "postgresdb";
const string adminPostgresDb = "admin-postgresdb";
const string yarp = "ingress";

var powerBillingUsagePostgres = builder.AddPostgres(postgres)
    .WithImage("postgres")
    .WithImageTag("latest")
    //.WithPgAdmin()
    //.WithDataVolume("postgres-data")
    .WithLifetime(ContainerLifetime.Session)
    ;

var postgresdb = powerBillingUsagePostgres.AddDatabase(postgresDb);
//var postgresAdmindb = powerBillingUsagePostgres.AddDatabase(adminPostgresDb);

var redis = builder.AddRedis(redisCache)
    .WithImage("ghcr.io/microsoft/garnet")
    .WithImageTag("latest")
    .WithEndpoint(name: "redis-tcp", port: 6379, targetPort: 6379)
    .WithLifetime(ContainerLifetime.Session)
    ;

var migration = builder.AddProject<Projects.PowerBillingUsage_DbMigrator>(PowerBillingUsage_DbMigrator)
    .WithReference(postgresdb)
    //.WithReference(postgresAdmindb)
    .WaitFor(postgresdb, WaitBehavior.WaitOnResourceUnavailable)
    ;

var powerBillingUsageApi = builder.AddProject<Projects.PowerBillingUsage_API>(PowerBillingUsage_API_Name)
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb)
    //.WithReference(postgresAdmindb)
    .WithReference(redis)
    .WaitFor(postgresdb, WaitBehavior.WaitOnResourceUnavailable)
    .WaitFor(redis, WaitBehavior.WaitOnResourceUnavailable)
    .WaitForCompletion(migration)
    ;

var powerBillingUsage_Web = builder.AddProject<Projects.PowerBillingUsage_Web>(PowerBillingUsage_Web_Name)
    .WithReference(powerBillingUsageApi)
    .WithReference(redis)
    ;

var powerBillingUsageYarp = builder.AddYarp(yarp)
    .WithHttpsEndpoint(port: 8001, targetPort: 7046)
    .WithReference(powerBillingUsageApi)
    .LoadFromConfiguration("ReverseProxy")
    ;

var app = builder.Build();

app.Run();