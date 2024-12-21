using Admin.Application.Orders.DTOs;
using Admin.Application.Orders.Queries;
using Admin.Domain.Enums;
using Admin.WebAPI.Endpoints.Orders.Responses;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Orders.GetById;
public record GetOrderRequest(Guid Id);
public class GetOrderEndpoint : Endpoint<GetOrderRequest, OrderResponse>
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
            .Produces<OrderResponse>(StatusCodes.Status200OK)
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
            var response = new OrderResponse(result.Value);
            await SendOkAsync(response, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}


