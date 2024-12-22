using Admin.WebAPI.Hubs;

namespace Admin.WebAPI.Infrastructure;

public static class SignalRConfiguration
{
    public static IEndpointRouteBuilder MapSignalRHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<ProductHub>("/hubs/products");
        endpoints.MapHub<OrderHub>("/hubs/order");
        endpoints.MapHub<ProductVariantHub>("/hubs/productvariant");
        return endpoints;
    }
}
