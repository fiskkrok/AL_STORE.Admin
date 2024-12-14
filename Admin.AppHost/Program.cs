using Aspire.Hosting;

using IdentityModel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client.Extensions.Msal;

var builder = DistributedApplication.CreateBuilder(args);


var storage = builder.AddAzureStorage("StorageConnectionString");
    storage.RunAsEmulator(r =>
    {
            r.WithDataVolume();
    });
    var blobStorage = storage.AddBlobs("alstoreblob");
var redis = builder.AddRedis("nice_kare").WithRedisInsight();
var rabbitMq = builder.AddRabbitMQ("my-rabbit")
    .WithLifetime(ContainerLifetime.Persistent);
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);
var adminDb = sqlServer.AddDatabase("admindb");

var identityApi = builder.AddProject<Projects.IdentityServer>("identityserver");
builder.AddProject<Projects.Admin_WebAPI>("admin-webapi").WithReference(redis)
    .WithReference(rabbitMq).WaitFor(rabbitMq).WithReference(adminDb).WithReference(blobStorage);

builder.Build().Run();
