using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public class DeleteProductVariantEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteProductVariantEndpoint> _logger;

    public DeleteProductVariantEndpoint(IMediator mediator, ILogger<DeleteProductVariantEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/products/{productId}/variants/{variantId}");
        Description(d => d
            .WithTags("Product Variants")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteProductVariant")
            .WithOpenApi());
        Version(1);
        Claims("products.edit");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new DeleteProductVariantCommand(
            Route<Guid>("productId"),
            Route<Guid>("variantId"));

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
}
