using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record DeleteProductVariantRequest
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}

public class DeleteProductVariantEndpoint : Endpoint<DeleteProductVariantRequest, IResult>
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
        Delete("/products/{ProductId}/variants/{VariantId}");
        Description(d => d
            .WithTags("Product Variants")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteProductVariant")
            .WithOpenApi());
        Version(1);
        Claims("products.edit");
    }

    public override async Task HandleAsync(DeleteProductVariantRequest req, CancellationToken ct)
    {
        var command = new DeleteProductVariantCommand(req.ProductId, req.VariantId);
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