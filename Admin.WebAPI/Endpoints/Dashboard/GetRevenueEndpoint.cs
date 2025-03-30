using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Admin.WebAPI.Infrastructure.Authorization;

namespace Admin.WebAPI.Endpoints.Dashboard;

public class GetRevenueEndpoint : EndpointWithoutRequest<object>
{
    private readonly ILogger<GetRevenueEndpoint> _logger;

    public GetRevenueEndpoint(ILogger<GetRevenueEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/dashboard/revenue");
        Description(d => d
            .WithTags("Dashboard")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetRevenue")
            .WithOpenApi());
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Policies(AuthConstants.CanReadProductsPolicy);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // For demo purposes, providing mock data
            var data = new
            {
                total = 24680.50m,
                trend = GenerateMockRevenueTrend()
            };

            await SendAsync(data, StatusCodes.Status200OK, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revenue data");
            await SendErrorsAsync(500, ct);
        }
    }

    private object[] GenerateMockRevenueTrend()
    {
        var startDate = DateTime.Now.AddDays(-30);
        var random = new Random(123); // Fixed seed for consistent demo data

        return Enumerable.Range(0, 30).Select(i =>
        {
            var date = startDate.AddDays(i);
            var amount = 500 + random.Next(0, 800);

            return new
            {
                date = date.ToString("yyyy-MM-dd"),
                amount = amount
            };
        }).ToArray();
    }
}