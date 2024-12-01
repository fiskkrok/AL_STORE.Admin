using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Infrastructure.Common.Exceptions;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;

namespace Admin.Infrastructure.Services.FileStorage;

public class AzureBlobStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

public class AzureBlobStorageService : IFileStorage
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IOptions<AzureBlobStorageSettings> settings,
        BlobServiceClient blobServiceClient,
        ILogger<AzureBlobStorageService> logger)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(
            settings.Value.ContainerName);
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(
        FileUploadRequest file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure container exists
            await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            // Create a unique file name with original extension
            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid()}{extension}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            var headers = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            var retryPolicy = Policy
                .Handle<RequestFailedException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                await blobClient.UploadAsync(
                    file.Content,
                    new BlobUploadOptions { HttpHeaders = headers },
                    cancellationToken);
            });

            return new FileUploadResult
            {
                Url = blobClient.Uri.ToString(),
                FileName = blobName,
                Size = file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} to blob storage", file.FileName);
            throw new InfrastructureException("Failed to upload file to blob storage", ex);
        }
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var retryPolicy = Policy
                .Handle<RequestFailedException>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file with URL {FileUrl} from blob storage", fileUrl);
            throw new InfrastructureException("Failed to delete file from blob storage", ex);
        }
    }
}