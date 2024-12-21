using Admin.Application.Products.DTOs;

namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record ProductVariantResponse
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Stock { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();

    public static ProductVariantResponse FromDto(ProductVariantDto dto) =>
        new()
        {
            Id = dto.Id,
            Sku = dto.Sku,
            Price = dto.Price,
            Currency = dto.Currency,
            Stock = dto.Stock,
            Attributes = dto.Attributes
        };
}
