namespace Admin.Application.Products.DTOs;
public record ProductVariantDto
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal? CompareAtPrice { get; init; }
    public decimal? CostPrice { get; init; }
    public string? Barcode { get; init; }
    public int Stock { get; init; }
    public bool TrackInventory { get; init; } = true;
    public bool AllowBackorders { get; init; } = false;
    public int? LowStockThreshold { get; init; }
    public int SortOrder { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = [];
    public List<ProductImageDto> Images { get; init; } = [];
    public Guid ProductId { get; init; }
}
