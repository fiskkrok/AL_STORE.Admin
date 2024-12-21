using Admin.Application.Common.Models;
using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

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
        Post("/products/{productId}/variants");
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
                Route<Guid>("productId"),
                req.Sku,
                req.Price,
                req.Currency,
                req.Stock,
                req.Attributes.Select(o => new ProductAttributeRequest()
                {
                    Name = o.Name,
                    Value = o.Value,
                    Type = o.Type
                }).ToList());

            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                await SendCreatedAtAsync<GetProductVariantEndpoint>(
                    new { productId = Route<Guid>("productId"), variantId = result.Value },
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
