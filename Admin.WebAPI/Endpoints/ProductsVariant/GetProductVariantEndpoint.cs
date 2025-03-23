using Admin.Application.ProductVariants.Queries;
using Admin.Application.Products.DTOs;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record GetProductVariantRequest
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}

public class GetProductVariantEndpoint : Endpoint<GetProductVariantRequest, ProductVariantDto>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetProductVariantEndpoint> _logger;

    public GetProductVariantEndpoint(IMediator mediator, ILogger<GetProductVariantEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/products/{ProductId}/variants/{VariantId}");
        Description(d => d
            .WithTags("Product Variants")
            .Produces<ProductVariantDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetProductVariant")
            .WithOpenApi());
        Version(1);
        Claims("products.read");
    }

    public override async Task HandleAsync(GetProductVariantRequest req, CancellationToken ct)
    {
        var query = new GetProductVariantQuery(req.ProductId, req.VariantId);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}