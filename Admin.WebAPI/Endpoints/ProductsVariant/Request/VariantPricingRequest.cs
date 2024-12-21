namespace Admin.WebAPI.Endpoints.ProductsVariant.Request;

public record VariantPricingRequest
{
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? CompareAtPrice { get; init; }
    public decimal? CostPrice { get; init; }
}
