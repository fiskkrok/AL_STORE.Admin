using Admin.Application;
using Admin.Infrastructure;
using Admin.Infrastructure.Services.FileStorage;
using Admin.WebAPI.Infrastructure;
using Microsoft.Extensions.Azure;
using Admin.WebAPI.Extensions;
using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

// Add Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebAPI();
builder.Services.AddMessagingInfrastructure(builder.Configuration);
builder.Services.AddAzureClients(clientBuilder =>
{
    var blobStorageSettings = builder.Configuration.GetSection("AzureBlobStorageSettings").Get<AzureBlobStorageSettings>();

    if (blobStorageSettings?.ConnectionString == "UseDevelopmentStorage=true")
    {
        clientBuilder.AddBlobServiceClient(
            new Uri("http://127.0.0.1:10000/devstoreaccount1"));
    }
    else
    {
        clientBuilder.AddBlobServiceClient(blobStorageSettings?.ConnectionString ??
                                           throw new InvalidOperationException("Blob storage connection string not configured."));
    }
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Admin_";
});

// Add HybridCache
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromHours(24),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
    options.MaximumPayloadBytes = 5 * 1024 * 1024; // 5MB

});
#pragma warning restore EXTEXP0018




var app = builder.Build();

await app.AddPipeline();


await app.RunAsync();
