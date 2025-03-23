using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record CreateProductVariantRequest
{
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int Stock { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
}

public class CreateProductVariantEndpoint : Endpoint<CreateProductVariantRequest, Guid>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateProductVariantEndpoint> _logger;

    public CreateProductVariantEndpoint(IMediator mediator, ILogger<CreateProductVariantEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/products/{ProductId}/variants");
        Description(d => d
            .WithTags("Product Variants")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateProductVariant")
            .WithOpenApi());
        Version(1);
        Claims("products.edit");
    }

    public override async Task HandleAsync(CreateProductVariantRequest req, CancellationToken ct)
    {
        try
        {
            var command = new CreateProductVariantCommand(
                Route<Guid>("ProductId"),
                req.Sku,
                req.Price,
                req.Currency,
                req.Stock,
                req.Attributes);

            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                await SendCreatedAtAsync<GetProductVariantEndpoint>(
                    new { ProductId = Route<Guid>("ProductId"), VariantId = result.Value },
                    result.Value,
                    generateAbsoluteUrl: true,
                    cancellation: ct);
            }
            else
            {
                await SendErrorsAsync(400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product variant");
            await SendErrorsAsync(500, ct);
        }
    }
}