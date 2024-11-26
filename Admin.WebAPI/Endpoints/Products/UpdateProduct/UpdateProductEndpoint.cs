using Admin.Application.Products.Commands.UpdateProduct;
using Admin.WebAPI.Endpoints.Products.Request;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Products.UpdateProduct;

public class UpdateProductEndpoint : Endpoint<UpdateProductRequest, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateProductEndpoint> _logger;

    public UpdateProductEndpoint(IMediator mediator, ILogger<UpdateProductEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/products/{Id}");
        AllowFileUploads();
        Description(d => d
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("UpdateProduct")
            .WithOpenApi());
        //Claims("products.update", "api.full");
        //Roles("SystemAdministrator");
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        try
        {
            var command = new UpdateProductCommand
            {
                Id = req.Id,
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Currency = req.Currency,
                CategoryId = req.CategoryId,
                SubCategoryId = req.SubCategoryId,
                NewImages = req.NewImages,
                ImageIdsToRemove = req.ImageIdsToRemove
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                await SendErrorsAsync(400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}
