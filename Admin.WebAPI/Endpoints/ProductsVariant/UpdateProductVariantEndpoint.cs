using Admin.Application.Products.DTOs;
using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record UpdateProductVariantRequest
{
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int Stock { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
}

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
        Put("/products/{ProductId}/variants/{VariantId}");
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
                ProductId = Route<Guid>("ProductId"),
                VariantId = Route<Guid>("VariantId"),
                Sku = req.Sku,
                Price = req.Price,
                Currency = req.Currency,
                Stock = req.Stock,
                Attributes = req.Attributes
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