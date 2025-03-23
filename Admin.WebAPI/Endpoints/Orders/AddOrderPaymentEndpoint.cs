using Admin.Application.Orders.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Orders;

public class AddOrderPaymentEndpoint : Endpoint<AddPaymentCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<AddOrderPaymentEndpoint> _logger;

    public AddOrderPaymentEndpoint(IMediator mediator, ILogger<AddOrderPaymentEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/orders/{OrderId}/payments");
        Description(d => d
            .WithTags("Orders")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("AddOrderPayment")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(AddPaymentCommand req, CancellationToken ct)
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