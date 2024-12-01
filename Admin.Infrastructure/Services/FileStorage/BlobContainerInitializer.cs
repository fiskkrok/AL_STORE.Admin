using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Services.FileStorage;
// Add this new class
public class BlobContainerInitializer : IHostedService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IOptions<AzureBlobStorageSettings> _settings;
    private readonly ILogger<BlobContainerInitializer> _logger;

    public BlobContainerInitializer(
        BlobServiceClient blobServiceClient,
        IOptions<AzureBlobStorageSettings> settings,
        ILogger<BlobContainerInitializer> logger)
    {
        _blobServiceClient = blobServiceClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.Value.ContainerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            // Optionally set public access level
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            _logger.LogInformation("Blob container {ContainerName} initialized", _settings.Value.ContainerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing blob container {ContainerName}", _settings.Value.ContainerName);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
