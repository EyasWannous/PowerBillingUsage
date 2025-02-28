var builder = DistributedApplication.CreateBuilder(args);

const string PowerBillingUsage_API_Name = "powerbillingusage-api";
const string PowerBillingUsage_Web_Name = "powerbillingusage-blazor";
const string redisCache = "rediscache";
const string postgresDb = "postgresdb";

var postgres = builder.AddPostgres(postgresDb)
    .WithImage("postgres")
    .WithImageTag("latest")
    .WithPgAdmin()
    ;

var redis = builder.AddRedis(redisCache)
    .WithImage("ghcr.io/microsoft/garnet")
    .WithImageTag("latest")
    ;

var powerBillingUsageApi = builder.AddProject<Projects.PowerBillingUsage_API>(PowerBillingUsage_API_Name)
    .WithReference(postgres)
    .WithReference(redis)
    ;

var powerBillingUsage_Web = builder.AddProject<Projects.PowerBillingUsage_Web>(PowerBillingUsage_Web_Name)
    .WithReference(powerBillingUsageApi)
    .WithReference(redis)
    ;

builder.Build().Run();
