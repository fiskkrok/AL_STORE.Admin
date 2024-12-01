using Admin.Application;
using Admin.Infrastructure;
using Admin.Infrastructure.Services.FileStorage;
using Admin.WebAPI.Infrastructure;
using Microsoft.Extensions.Azure;
using Admin.WebAPI.Extensions;

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

var app = builder.Build();

await app.AddPipeline();


await app.RunAsync();
