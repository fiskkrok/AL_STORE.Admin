using Admin.Application.Orders.Commands;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Orders.Update;

public class UpdateOrderShippingEndpoint : Endpoint<UpdateShippingInfoCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateOrderShippingEndpoint> _logger;

    public UpdateOrderShippingEndpoint(IMediator mediator, ILogger<UpdateOrderShippingEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/orders/{OrderId}/shipping");
        Description(d => d
            .WithTags("Orders")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateOrderShipping")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(UpdateShippingInfoCommand req, CancellationToken ct)
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
