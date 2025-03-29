using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Services.FileStorage;

using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Storage services configuration
/// </summary>
public static class StorageServicesConfiguration
{
    /// <summary>
    /// Adds file storage services, including Azure Blob Storage
    /// </summary>
    public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Azure Blob Storage settings
        services.Configure<AzureBlobStorageSettings>(configuration.GetSection("AzureBlobStorageSettings"));

        // Add Azure Blob Storage client
        services.AddAzureBlobClient(configuration);

        // Register the file storage service
        services.AddScoped<IFileStorage, AzureBlobStorageService>();

        // Add the blob container initializer
        services.AddHostedService<BlobContainerInitializer>();

        return services;
    }

    /// <summary>
    /// Adds Azure Blob Storage client
    /// </summary>
    private static IServiceCollection AddAzureBlobClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureBlobStorageSettings>>().Value;

            // For local development with Azurite
            if (settings.ConnectionString == "UseDevelopmentStorage=true")
            {
                return new BlobServiceClient(
                    new Uri("http://localhost:10000/devstoreaccount1"),
                    new StorageSharedKeyCredential(
                        "devstoreaccount1",
                        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
            }

            // Production/staging environment
            return new BlobServiceClient(settings.ConnectionString);
        });

        return services;
    }
}