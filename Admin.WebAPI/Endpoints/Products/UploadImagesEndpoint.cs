using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;

using FastEndpoints;

namespace Admin.WebAPI.Endpoints.Products;

public class UploadImagesEndpoint : EndpointWithoutRequest<List<FileUploadResult>>
{
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<UploadImagesEndpoint> _logger;

    public UploadImagesEndpoint(IFileStorage fileStorage, ILogger<UploadImagesEndpoint> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/products/upload-images");
        AllowAnonymous();
        AllowFileUploads();
        Description(d => d
            .WithTags("Products")
            .Produces<List<string>>(200)
            .Produces(400)
            .WithName("UploadProductImages")
            .WithOpenApi());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var files = HttpContext.Request.Form.Files;
            _logger.LogInformation($"Received {files.Count} files");

            var results = new List<FileUploadResult>();
            foreach (var file in files)
            {
                _logger.LogInformation($"Processing file: {file.FileName}, Size: {file.Length}, Type: {file.ContentType}");

                var uploadRequest = new FileUploadRequest
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    Content = file.OpenReadStream()
                };

                var result = await _fileStorage.UploadAsync(uploadRequest, ct);
                results.Add(result);
            }

            await SendOkAsync(results, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading product images");
            await SendErrorsAsync(400, ct);
        }
    }
}