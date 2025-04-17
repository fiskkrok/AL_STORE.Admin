using Admin.Application.Products.Commands.UpdateProduct;
using Admin.Application.Products.DTOs;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Products;

public class UpdateProductEndpoint : Endpoint<UpdateProductCommand, IResult>
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
        Description(d => d
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("UpdateProduct")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(UpdateProductCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                // Handle specific error cases
                if (result.Error?.Code == "Product.NotFound" ||
                    result.Error?.Code == "Category.NotFound" ||
                    result.Error?.Code == "SubCategory.NotFound" ||
                    result.Error?.Code == "ProductType.NotFound")
                {
                    await SendNotFoundAsync(ct);
                }
                else
                {
                    await SendErrorsAsync(400, ct);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", command.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}