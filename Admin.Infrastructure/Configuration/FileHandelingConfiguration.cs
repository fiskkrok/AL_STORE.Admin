using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Services.FileStorage;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Configuration;
public static class FileHandelingConfiguration
{
    public static IServiceCollection AddFileHandelingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureBlobStorageSettings>(
            configuration.GetSection("AzureBlobStorageSettings"));
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureBlobStorageSettings>>().Value;

            if (settings.ConnectionString == "UseDevelopmentStorage=true")
            {
                // Local development using Azurite
                return new BlobServiceClient(
                    new Uri("http://127.0.0.1:10000/devstoreaccount1"),
                    new StorageSharedKeyCredential(
                        "devstoreaccount1",
                        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
            }

            // Production/staging environment
            return new BlobServiceClient(settings.ConnectionString);
        });
        services.AddScoped<IFileStorage, AzureBlobStorageService>();
        // Ensure the blob container exists
        services.AddHostedService<BlobContainerInitializer>();
        return services;
    }
}
