using Admin.Application.Orders.Queries;
using Admin.Application.Orders.DTOs;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Orders;

public record GetOrderRequest
{
    public Guid Id { get; init; }
}

public class GetOrderEndpoint : Endpoint<GetOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetOrderEndpoint> _logger;

    public GetOrderEndpoint(IMediator mediator, ILogger<GetOrderEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/orders/{Id}");
        Description(d => d
            .WithTags("Orders")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetOrder")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(GetOrderRequest req, CancellationToken ct)
    {
        var query = new GetOrderQuery(req.Id);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}