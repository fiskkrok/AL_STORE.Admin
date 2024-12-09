using Admin.Application.ProductVariants.Queries;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Products.ProductsVariant;

public class GetProductVariantEndpoint : Endpoint<GetProductVariantRequest, ProductVariantResponse>
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
        Get("/products/{productId}/variants/{variantId}");
        Description(d => d
            .WithTags("Product Variants")
            .Produces<ProductVariantResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetProductVariant")
            .WithOpenApi());
        Version(1);
        Claims("products.read");
    }

    public override async Task HandleAsync(GetProductVariantRequest req, CancellationToken ct)
    {
        var query = new GetProductVariantQuery(Route<Guid>("productId"), Route<Guid>("variantId"));
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            var response = new ProductVariantResponse();
            
            await SendOkAsync(response, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}