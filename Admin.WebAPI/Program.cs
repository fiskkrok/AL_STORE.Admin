using Admin.Application;
using Admin.Application.Common.Interfaces;
using Admin.Infrastructure;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Services.FileStorage;
using Admin.WebAPI.Infrastructure;
using Microsoft.Extensions.Azure;
using Admin.WebAPI.Extensions;
using Admin.WebAPI.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Admin.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

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

// For Aspire
//builder.AddAzureBlobClient("alstoreblob");
//builder.AddRedisClient("redis");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Admin_";
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Decorate the product repository with caching

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
builder.Services.AddCustomHealthChecks(builder.Configuration);



var app = builder.Build();

//app.MapDefaultEndpoints();

await app.AddPipeline();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Info = report.Entries.Select(e => new
            {
                Key = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration,
                Description = e.Value.Description,
                Data = e.Value.Data
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});
app.UseErrorHandling();
// Detailed health check for specific components
app.MapHealthChecks("/health/cache", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("cache")
});

app.MapHealthChecks("/health/database", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("database")
});

app.MapHealthChecks("/health/messaging", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("messaging")
});
await app.RunAsync();
