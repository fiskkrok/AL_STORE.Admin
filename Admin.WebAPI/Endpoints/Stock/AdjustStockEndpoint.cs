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
        Post("/api/stock/{ProductId}/adjust");
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
            
        // After successful stock adjustment, notify connected clients via SignalR
        var updatedStock = await _mediator.Send(new GetStockItemQuery(req.ProductId), ct);
        if (updatedStock.IsSuccess)
        {
            await _stockHub.Clients.All.SendAsync("StockUpdated", updatedStock.Value, ct);
            
            // If stock is low, send low stock alert
            if (updatedStock.Value.IsLowStock)
            {
                await _stockHub.Clients.All.SendAsync("LowStockAlert", updatedStock.Value, ct);
            }
        }
            
        await SendOkAsync(ct);
    }
}