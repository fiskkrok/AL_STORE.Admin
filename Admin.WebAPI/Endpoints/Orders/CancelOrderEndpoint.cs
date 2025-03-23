using Admin.Application.Orders.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Orders;

public class CancelOrderEndpoint : Endpoint<CancelOrderCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CancelOrderEndpoint> _logger;

    public CancelOrderEndpoint(IMediator mediator, ILogger<CancelOrderEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/orders/{OrderId}/cancel");
        Description(d => d
            .WithTags("Orders")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("CancelOrder")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(CancelOrderCommand req, CancellationToken ct)
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
}
