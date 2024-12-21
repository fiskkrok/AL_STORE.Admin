using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public class UpdateProductVariantEndpoint : Endpoint<UpdateProductVariantRequest, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateProductVariantEndpoint> _logger;

    public UpdateProductVariantEndpoint(IMediator mediator, ILogger<UpdateProductVariantEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/products/{productId}/variants/{variantId}");
        Description(d => d
            .WithTags("Product Variants")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateProductVariant")
            .WithOpenApi());
        Version(1);
        Claims("products.edit");
    }

    public override async Task HandleAsync(UpdateProductVariantRequest req, CancellationToken ct)
    {
        try
        {
            var command = new UpdateProductVariantCommand
            {
                ProductId = Route<Guid>("productId"),
                VariantId = Route<Guid>("variantId"),
                Sku = req.Sku,
                Price = req.Price,
                Currency = req.Currency,
                Stock = req.Stock,
                Attributes = req.Attributes.Select(o => new Application.Common.Models.ProductAttributeRequest()
                {
                    Name = o.Name,
                    Value = o.Value,
                    Type = o.Type
                }).ToList()
            };

            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                await SendNotFoundAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product variant");
            await SendErrorsAsync(500, ct);
        }
    }
}