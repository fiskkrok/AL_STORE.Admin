using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Admin.WebAPI.Infrastructure.Authorization;

namespace Admin.WebAPI.Endpoints.Dashboard;

public class GetShippingEndpoint : EndpointWithoutRequest<object>
{
    private readonly ILogger<GetShippingEndpoint> _logger;

    public GetShippingEndpoint(ILogger<GetShippingEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/dashboard/shipping");
        Description(d => d
            .WithTags("Dashboard")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetShipping")
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
                pending = 23,
                late = 5,
                pendingOrders = GenerateMockPendingOrders()
            };

            await SendAsync(data, StatusCodes.Status200OK, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipping data");
            await SendErrorsAsync(500, ct);
        }
    }

    private object[] GenerateMockPendingOrders()
    {
        var random = new Random(789); // Fixed seed for consistent demo data

        return Enumerable.Range(1, 10).Select(i =>
        {
            var createdAt = DateTime.Now.AddHours(-random.Next(1, 72));

            return new
            {
                id = Guid.NewGuid().ToString(),
                orderNumber = $"ORD-{10000 + i}",
                createdAt = createdAt.ToString("yyyy-MM-dd HH:mm"),
                status = "pending"
            };
        }).ToArray();
    }
}