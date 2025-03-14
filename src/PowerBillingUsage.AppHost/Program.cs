var builder = DistributedApplication.CreateBuilder(args);

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
    .WithPgAdmin()
    ;

var postgresdb = powerBillingUsagePostgres.AddDatabase(postgresDb);
var postgresAdmindb = powerBillingUsagePostgres.AddDatabase(adminPostgresDb);

//var sql = builder.AddSqlServer("powerbillingusage-sqlserver")
//    .WithDataVolume()
//    .AddDatabase("powerbillingusage-sqldb");

//builder.AddProject<Projects.PowerBillingUsage_MigrationService>("migrations")
//    .WithReference(sql)
//    .WithReference(postgresdb)
//    .WithReference(postgresAdmindb);

var redis = builder.AddRedis(redisCache)
    .WithImage("ghcr.io/microsoft/garnet")
    .WithImageTag("latest")
    ;

var powerBillingUsageApi = builder.AddProject<Projects.PowerBillingUsage_API>(PowerBillingUsage_API_Name)
    //.WithReference(sql)
    .WithReference(postgresdb)
    .WithReference(postgresAdmindb)
    .WithReference(redis)
    .WithExternalHttpEndpoints()
    ;

var powerBillingUsage_Web = builder.AddProject<Projects.PowerBillingUsage_Web>(PowerBillingUsage_Web_Name)
    .WithReference(powerBillingUsageApi)
    .WithReference(redis)
    ;

var powerBillingUsageYarp = builder.AddYarp(yarp)
    .WithEndpoint(port: 8001, scheme: "https", targetPort: 7046)
    .WithReference(powerBillingUsageApi)
    .LoadFromConfiguration("ReverseProxy")
    ;

var app = builder.Build();

app.Run();