//using FastEndpoints;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Admin.WebAPI.Infrastructure.Authorization;

//namespace Admin.WebAPI.Endpoints.Dashboard;

//public class GetOrdersEndpoint : EndpointWithoutRequest<object>
//{
//    private readonly ILogger<GetOrdersEndpoint> _logger;

//    public GetOrdersEndpoint(ILogger<GetOrdersEndpoint> logger)
//    {
//        _logger = logger;
//    }

//    public override void Configure()
//    {
//        Get("/dashboard/orders");
//        Description(d => d
//            .WithTags("Dashboard")
//            .Produces<object>(StatusCodes.Status200OK)
//            .Produces(StatusCodes.Status400BadRequest)
//            .WithName("GetOrders")
//            .WithOpenApi());
//        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
//        Policies(AuthConstants.CanReadProductsPolicy);
//    }

//    public override async Task HandleAsync(CancellationToken ct)
//    {
//        try
//        {
//            // For demo purposes, providing mock data
//            var data = new
//            {
//                total = 389,
//                trend = GenerateMockOrderTrend(),
//                byStatus = new[]
//                {
//                    new { status = "pending", value = 45, color = "#FCD34D" },
//                    new { status = "confirmed", value = 98, color = "#60A5FA" },
//                    new { status = "processing", value = 67, color = "#818CF8" },
//                    new { status = "shipped", value = 120, color = "#34D399" },
//                    new { status = "delivered", value = 42, color = "#10B981" },
//                    new { status = "cancelled", value = 17, color = "#EF4444" }
//                }
//            };

//            await SendAsync(data, StatusCodes.Status200OK, ct);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error retrieving orders data");
//            await SendErrorsAsync(500, ct);
//        }
//    }

//    private object[] GenerateMockOrderTrend()
//    {
//        var startDate = DateTime.Now.AddDays(-30);
//        var random = new Random(456); // Fixed seed for consistent demo data

//        return Enumerable.Range(0, 30).Select(i =>
//        {
//            var date = startDate.AddDays(i);
//            var count = 5 + random.Next(0, 20);

//            return new
//            {
//                date = date.ToString("yyyy-MM-dd"),
//                count = count
//            };
//        }).ToArray();
//    }
//}