namespace Admin.Application.Products.DTOs;
public record ProductVariantDto
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Stock { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
}
