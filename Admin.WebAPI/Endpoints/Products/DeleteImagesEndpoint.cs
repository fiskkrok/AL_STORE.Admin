using Admin.Application.Common.Interfaces;

using FastEndpoints;

namespace Admin.WebAPI.Endpoints.Products;

public class DeleteImagesEndpoint : Endpoint<DeleteImagesRequest, IResult>
{
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<DeleteImagesEndpoint> _logger;

    public DeleteImagesEndpoint(IFileStorage fileStorage, ILogger<DeleteImagesEndpoint> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/products/delete-images");
        Description(d => d
            .WithTags("Images")
            .Produces<Guid>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("DeleteProductImage")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(DeleteImagesRequest req, CancellationToken ct)
    {
        try
        {
            foreach (var imageId in req.ImageIds)
            {
                await _fileStorage.DeleteAsync(imageId, ct);
            }

            await SendNoContentAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting images");
            await SendErrorsAsync(400, ct);
        }
    }
}

public record DeleteImagesRequest
{
    public List<string> ImageIds { get; set; } = new();
}
