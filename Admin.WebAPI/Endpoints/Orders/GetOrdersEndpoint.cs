using Admin.Application.Orders.Queries;
using Admin.Domain.Enums;
using Admin.WebAPI.Endpoints.Orders.Responses;
using Admin.WebAPI.Endpoints.Products.Models;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Orders;

public class GetOrdersEndpoint : EndpointWithoutRequest<PagedResponse<OrderResponse>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetOrdersEndpoint> _logger;

    public GetOrdersEndpoint(IMediator mediator, ILogger<GetOrdersEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/orders");
        Description(d => d
            .WithTags("Orders")
            .Produces<PagedResponse<OrderResponse>>(StatusCodes.Status200OK)
            .WithName("GetOrders")
            .WithOpenApi());
        AllowAnonymous(); // TODO: Update with proper authorization
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetOrdersQuery
        {
            CustomerId = Query<Guid?>("customerId", isRequired: false),
            Status = Query<OrderStatus?>("status", isRequired: false),
            FromDate = Query<DateTime?>("fromDate", isRequired: false),
            ToDate = Query<DateTime?>("toDate", isRequired: false),
            MinTotal = Query<decimal?>("minTotal", isRequired: false),
            MaxTotal = Query<decimal?>("maxTotal", isRequired: false),
            SearchTerm = Query<string>("search", isRequired: false),
            SortBy = Query<string>("sortBy", isRequired: false),
            SortDescending = Query<bool>("sortDescending", isRequired: false),
            PageNumber = Query<int>("page", isRequired: false) <= 0 ? 1 : Query<int>("page", isRequired: false),
            PageSize = Query<int>("pageSize", isRequired: false) <= 0 ? 10 : Query<int>("pageSize", isRequired: false)
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            var response = PagedResponse<OrderResponse>.FromPagedList(
                result.Value,
                dto => new OrderResponse(dto));

            await SendOkAsync(response, ct);
        }
        else
        {
            await SendErrorsAsync(400, ct);
        }
    }
}
