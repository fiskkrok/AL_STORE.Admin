using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public class UpdateVariantInventoryEndpoint : Endpoint<UpdateVariantInventoryCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateVariantInventoryEndpoint> _logger;

    public UpdateVariantInventoryEndpoint(
        IMediator mediator,
        ILogger<UpdateVariantInventoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/products/{ProductId}/variants/{VariantId}/inventory");
        Description(d => d
            .WithTags("Product Variants")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateVariantInventory")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(
        UpdateVariantInventoryCommand req,
        CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

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
            _logger.LogError(ex, "Error updating variant inventory");
            await SendErrorsAsync(500, ct);
        }
    }
}
