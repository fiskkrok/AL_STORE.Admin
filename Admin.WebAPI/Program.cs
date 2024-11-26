using Admin.Application;
using Admin.Infrastructure;
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
    clientBuilder.AddBlobServiceClient(
            builder.Configuration["StorageConnection:blobServiceUri"])
        .WithName("StorageConnection");
});

var app = builder.Build();

await app.AddPipeline();


await app.RunAsync();
