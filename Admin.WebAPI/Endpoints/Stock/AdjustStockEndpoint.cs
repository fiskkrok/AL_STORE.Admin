using Admin.Application.Inventory.Commands;
using Admin.Application.Inventory.Queries;
using Admin.WebAPI.Endpoints.Stock.Models;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Endpoints.Stock;

public class AdjustStockEndpoint : Endpoint<AdjustStockRequest, FastEndpoints.EmptyResponse>
{
    private readonly IMediator _mediator;
    private readonly IHubContext<StockHub> _stockHub;
    private readonly ILogger<AdjustStockEndpoint> _logger;

    public AdjustStockEndpoint(
        IMediator mediator,
        IHubContext<StockHub> stockHub,
        ILogger<AdjustStockEndpoint> logger)
    {
        _mediator = mediator;
        _stockHub = stockHub;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/stock/{ProductId}/adjust");
        Description(d => d
            .WithTags("Stock")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi());
        AuthSchemes(AuthConstants.JwtBearerScheme, AuthConstants.ApiKeyScheme);
        Policies(AuthConstants.CanManageProductsPolicy);
    }

    public override async Task HandleAsync(AdjustStockRequest req, CancellationToken ct)
    {
        var command = new AdjustStockCommand
        {
            ProductId = req.ProductId,
            Adjustment = req.Adjustment,
            Reason = req.Reason
        };

        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        // Get updated stock information
        var updatedStock = await _mediator.Send(new GetStockItemQuery(req.ProductId), ct);
        if (updatedStock.IsSuccess)
        {
            try
            {
                // Notify specific product group
                await _stockHub.Clients.Group($"stock-product-{req.ProductId}")
                    .SendAsync("StockUpdated", updatedStock.Value, cancellationToken: ct);

                // If stock is low, send low stock alert
                if (updatedStock.Value.IsLowStock)
                {
                    await _stockHub.Clients.Group("LowStockAlerts")
                        .SendAsync("LowStockAlert", updatedStock.Value, cancellationToken: ct);
                }

                // If out of stock, notify about that too
                if (updatedStock.Value.IsOutOfStock)
                {
                    await _stockHub.Clients.Group("LowStockAlerts")
                        .SendAsync("OutOfStockAlert", updatedStock.Value, cancellationToken: ct);
                }

                // Also notify everyone for dashboards
                await _stockHub.Clients.All.SendAsync("StockUpdated", updatedStock.Value, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                // Log but don't fail the request if SignalR notification fails
                _logger.LogError(ex, "Failed to send stock update notification for product {ProductId}", req.ProductId);
            }
        }

        await SendOkAsync(ct);
    }
}