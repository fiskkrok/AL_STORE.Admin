using Admin.Application.ProductVariants.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public class BatchUpdateVariantsEndpoint : Endpoint<BatchUpdateVariantsCommand, List<Guid>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<BatchUpdateVariantsEndpoint> _logger;

    public BatchUpdateVariantsEndpoint(
        IMediator mediator,
        ILogger<BatchUpdateVariantsEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/products/variants/batch");
        Description(d => d
            .WithTags("Product Variants")
            .Produces<List<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("BatchUpdateVariants")
            .WithOpenApi());
        Version(1);
        Claims("products.edit");
    }

    public override async Task HandleAsync(
        BatchUpdateVariantsCommand req,
        CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

            if (result.IsSuccess)
                await SendOkAsync(result.Value, ct);
            else
                await SendErrorsAsync(400, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch update variants");
            await SendErrorsAsync(500, ct);
        }
    }
}