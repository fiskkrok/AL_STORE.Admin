using Admin.Application.Orders.Commands;
using Admin.WebAPI.Endpoints.Orders.GetById;
using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Orders.Create;

public class CreateOrderEndpoint : Endpoint<CreateOrderCommand, Guid>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateOrderEndpoint> _logger;

    public CreateOrderEndpoint(IMediator mediator, ILogger<CreateOrderEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/orders");
        Description(d => d
            .WithTags("Orders")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateOrder")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(CreateOrderCommand req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

            if (result.IsSuccess)
            {
                await SendCreatedAtAsync<GetOrderEndpoint>(
                    new { id = result.Value },
                    result.Value,
                    generateAbsoluteUrl: true,
                    cancellation: ct);
            }
            else
            {
                await SendErrorsAsync(400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            await SendErrorsAsync(500, ct);
        }
    }
}
