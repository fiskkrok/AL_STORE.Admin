using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public class UpdateVariantPricingEndpoint : Endpoint<UpdateVariantPricingCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateVariantPricingEndpoint> _logger;

    public UpdateVariantPricingEndpoint(
        IMediator mediator,
        ILogger<UpdateVariantPricingEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/products/{ProductId}/variants/{VariantId}/pricing");
        Description(d => d
            .WithTags("Product Variants")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateVariantPricing")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(
        UpdateVariantPricingCommand req,
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
            _logger.LogError(ex, "Error updating variant pricing");
            await SendErrorsAsync(500, ct);
        }
    }
}
