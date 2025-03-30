using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Admin.WebAPI.Endpoints.Dashboard;

public class GetDashboardStatsEndpoint : EndpointWithoutRequest<DashboardStats>
{
    private readonly ILogger<GetDashboardStatsEndpoint> _logger;

    public GetDashboardStatsEndpoint(ILogger<GetDashboardStatsEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/products/stats");
        AllowAnonymous();
        Description(d => d
            .WithTags("Dashboard")
            .WithOpenApi()
            .Produces<DashboardStats>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var mockData = new DashboardStats
        {
            TotalProducts = 100,
            LowStockCount = 5,
            TotalCategories = 10,
            ActiveProductCount = 80
        };

        _logger.LogInformation("Returning mock data for DashboardStats");
        await SendOkAsync(mockData, ct);
    }
}

public class DashboardStats
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveProductCount { get; set; }
}