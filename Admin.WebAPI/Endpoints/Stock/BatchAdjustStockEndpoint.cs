using Admin.Application.Inventory.Commands;
using Admin.Application.Inventory.Queries;
using Admin.WebAPI.Endpoints.Stock.Models;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Endpoints.Stock;

public class BatchAdjustStockEndpoint : Endpoint<BatchAdjustStockRequest, FastEndpoints.EmptyResponse>
{
    private readonly IMediator _mediator;
    private readonly IHubContext<StockHub> _stockHub;
    private readonly ILogger<BatchAdjustStockEndpoint> _logger;

    public BatchAdjustStockEndpoint(
        IMediator mediator,
        IHubContext<StockHub> stockHub,
        ILogger<BatchAdjustStockEndpoint> logger)
    {
        _mediator = mediator;
        _stockHub = stockHub;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/stock/batch-adjust");
        Description(d => d
            .WithTags("Stock")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi());
        AuthSchemes(AuthConstants.JwtBearerScheme, AuthConstants.ApiKeyScheme);
        Policies(AuthConstants.CanManageProductsPolicy);
    }

    public override async Task HandleAsync(BatchAdjustStockRequest req, CancellationToken ct)
    {
        var command = new BatchAdjustStockCommand
        {
            Adjustments = req.Adjustments.Select(a => new StockAdjustment
            {
                ProductId = a.ProductId,
                Adjustment = a.Adjustment
            }).ToList(),
            Reason = req.Reason
        };
        
        var result = await _mediator.Send(command, ct);
        
        if (!result.IsSuccess)
        {
            await SendErrorsAsync(400, ct);
            return;
        }
        
        // After batch adjustment, get updated stock information for each adjusted product
        foreach (var adjustment in req.Adjustments)
        {
            var updatedStock = await _mediator.Send(new GetStockItemQuery(adjustment.ProductId), ct);
            if (updatedStock.IsSuccess)
            {
                await _stockHub.Clients.All.SendAsync("StockUpdated", updatedStock.Value, ct);
                
                // If stock is low, send low stock alert
                if (updatedStock.Value.IsLowStock)
                {
                    await _stockHub.Clients.All.SendAsync("LowStockAlert", updatedStock.Value, ct);
                }
            }
        }
            
        await SendOkAsync(ct);
    }
}