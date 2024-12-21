namespace Admin.WebAPI.Endpoints.ProductsVariant.Response;

public record VariantPricingResponse
{
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? CompareAtPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public decimal? Profit { get; init; }
    public decimal? Margin { get; init; }
}
