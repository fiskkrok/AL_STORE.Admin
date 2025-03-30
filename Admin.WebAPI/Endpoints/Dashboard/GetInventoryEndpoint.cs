using Admin.Application.Inventory.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Admin.WebAPI.Infrastructure.Authorization;

namespace Admin.WebAPI.Endpoints.Dashboard;

public class GetInventoryEndpoint : EndpointWithoutRequest<object>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetInventoryEndpoint> _logger;

    public GetInventoryEndpoint(IMediator mediator, ILogger<GetInventoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/dashboard/inventory");
        Description(d => d
            .WithTags("Dashboard")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetInventory")
            .WithOpenApi());
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Policies(AuthConstants.CanReadProductsPolicy);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var lowStockResult = await _mediator.Send(new GetLowStockItemsQuery());
            var outOfStockResult = await _mediator.Send(new GetOutOfStockItemsQuery());

            if (!lowStockResult.IsSuccess || !outOfStockResult.IsSuccess)
            {
                await SendAsync(new { message = "Failed to retrieve inventory data" }, StatusCodes.Status400BadRequest, ct);
                return;
            }

            var data = new
            {
                lowStock = lowStockResult.Value.Count,
                outOfStock = outOfStockResult.Value.Count,
                items = lowStockResult.Value.Take(5).Select(item => new
                {
                    id = item.ProductId.ToString(),
                    name = $"Product {item.ProductId.ToString()[..6]}",  // Mock name based on ID
                    sku = $"SKU-{item.ProductId.ToString()[..6]}",       // Mock SKU based on ID
                    currentStock = item.CurrentStock,
                    minimumStock = item.LowStockThreshold
                }).ToList()
            };

            await SendAsync(data, StatusCodes.Status200OK, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory dashboard data");
            await SendErrorsAsync(500, ct);
        }
    }
}