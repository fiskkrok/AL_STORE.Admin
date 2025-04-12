// Admin.WebAPI/Endpoints/Products/UpsertProductTypeEndpoint.cs
using Admin.Application.Products.Commands;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Products;

public class UpsertProductTypeEndpoint : Endpoint<UpsertProductTypeCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpsertProductTypeEndpoint> _logger;

    public UpsertProductTypeEndpoint(
        IMediator mediator,
        ILogger<UpsertProductTypeEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/products/types");
        Description(d => d
            .WithTags("ProductTypes")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi());
        // Add appropriate authorization - typically admin level
        Policies(AuthConstants.IsAdminPolicy);
    }

    public override async Task HandleAsync(UpsertProductTypeCommand req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

            if (result.IsSuccess)
            {
                await SendAsync(Results.Ok(new { id = result.Value }), cancellation: ct);
            }
            else
            {
                // Adjusted to use the correct overload of SendErrorsAsync
                await SendErrorsAsync( statusCode: 400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in upsert product type endpoint");
            await SendErrorsAsync( statusCode: 500, ct);
        }
    }
}