using Admin.Application.Orders.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Orders;

public class UpdateOrderStatusEndpoint : Endpoint<UpdateOrderStatusCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateOrderStatusEndpoint> _logger;

    public UpdateOrderStatusEndpoint(IMediator mediator, ILogger<UpdateOrderStatusEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/orders/{OrderId}/status");
        Description(d => d
            .WithTags("Orders")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateOrderStatus")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(UpdateOrderStatusCommand req, CancellationToken ct)
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
