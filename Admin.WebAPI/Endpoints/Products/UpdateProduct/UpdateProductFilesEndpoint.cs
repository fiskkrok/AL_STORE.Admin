using Admin.Application.Common.Models;
using Admin.Application.Products.Commands.UpdateProduct;
using Admin.WebAPI.Endpoints.Products.Request;
using FastEndpoints;
using MediatR;


namespace Admin.WebAPI.Endpoints.Products.UpdateProduct;

public class UpdateProductFilesEndpoint : Endpoint<UpdateProductRequest, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateProductFilesEndpoint> _logger;

    public UpdateProductFilesEndpoint(IMediator mediator, ILogger<UpdateProductFilesEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    public override void Configure()
    {
        Put("/products/{Id}/files");
        AllowFileUploads();
        // ... other config
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        // Handle file uploads here
        if (Files.Count > 0)
        {
            var command = new UpdateProductCommand
            {
                // ... set other properties
                NewImages = Files.Select(f => new FileUploadRequest
                {
                    FileName = f.FileName,
                    Length = f.Length,
                    ContentType = f.ContentType,
                    Content = f.OpenReadStream()
                }).ToList()
            };
            // ... rest of the handler


        }
    }
}
