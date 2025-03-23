using Admin.Application.Common.Models;

namespace Admin.WebAPI.Endpoints.ProductsVariant.Models;

public record CreateProductVariantRequest
{
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int Stock { get; init; }
    public List<ProductAttributeRequest> Attributes { get; init; } = new();
}