using Admin.Application.Common.Interfaces;
using Admin.Application.Inventory.Queries;
using Admin.Application.Products.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Admin.WebAPI.Infrastructure.Authorization;

namespace Admin.WebAPI.Endpoints.Statistics;

[HttpGet("/api/statistics")]
[Authorize(AuthenticationSchemes = $"{AuthConstants.JwtBearerScheme},{AuthConstants.ApiKeyScheme}", Policy = AuthConstants.CanReadProductsPolicy)]
public class GetStatisticsEndpoint : Endpoint<GetStatisticsRequest, StatisticsDto>
{
    private readonly IMediator _mediator;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<GetStatisticsEndpoint> _logger;

    public GetStatisticsEndpoint(IMediator mediator, ICategoryRepository categoryRepository, ILogger<GetStatisticsEndpoint> logger)
    {
        _mediator = mediator;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public override async Task HandleAsync(GetStatisticsRequest req, CancellationToken ct)
    {
        try
        {
            // Get count of all products
            var productsResult = await _mediator.Send(new GetProductCountQuery());
            
            // Get low stock items count
            var lowStockResult = await _mediator.Send(new GetLowStockItemsQuery());
            
            // Get out of stock items count
            var outOfStockResult = await _mediator.Send(new GetOutOfStockItemsQuery());
            
            // Get categories count
            var categories = await _categoryRepository.GetAllAsync(includeInactive: false);
            
            if (!productsResult.IsSuccess || !lowStockResult.IsSuccess || !outOfStockResult.IsSuccess)
            {
                await SendAsync(new StatisticsDto(), StatusCodes.Status400BadRequest, ct);
                return;
            }
            
            // Calculate active product count (all minus out of stock)
            int totalProducts = productsResult.Value;
            int outOfStockCount = outOfStockResult.Value.Count;
            int activeProductCount = totalProducts - outOfStockCount;
            
            var statistics = new StatisticsDto
            {
                TotalProducts = totalProducts,
                ActiveProductCount = activeProductCount,
                LowStockCount = lowStockResult.Value.Count,
                TotalCategories = categories.Count(),
                // These would come from order data in a real implementation
                TotalRevenue = GetMockRevenueForTimeRange(req.TimeRange),
                RevenueChange = GetMockRevenueChangeForTimeRange(req.TimeRange),
                TotalOrders = GetMockOrdersForTimeRange(req.TimeRange),
                OrdersChange = GetMockOrdersChangeForTimeRange(req.TimeRange)
            };
            
            await SendAsync(statistics, StatusCodes.Status200OK, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            await SendAsync(new StatisticsDto(), StatusCodes.Status500InternalServerError, ct);
        }
    }

    // Mock data helpers for demo purposes
    private decimal GetMockRevenueForTimeRange(string timeRange) => timeRange switch
    {
        "day" => 2468.50m,
        "week" => 15420.75m,
        "month" => 65280.25m,
        _ => 2468.50m
    };

    private decimal GetMockRevenueChangeForTimeRange(string timeRange) => timeRange switch
    {
        "day" => 12.5m,
        "week" => 8.3m,
        "month" => 15.7m,
        _ => 12.5m
    };

    private int GetMockOrdersForTimeRange(string timeRange) => timeRange switch
    {
        "day" => 38,
        "week" => 243,
        "month" => 1089,
        _ => 38
    };

    private decimal GetMockOrdersChangeForTimeRange(string timeRange) => timeRange switch
    {
        "day" => 5.2m,
        "week" => 7.8m,
        "month" => 12.1m,
        _ => 5.2m
    };
}

public class GetStatisticsRequest
{
    public string TimeRange { get; set; } = "day";
}