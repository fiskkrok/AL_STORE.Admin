using Admin.Application.Categories.DTOs;

namespace Admin.Application.Products.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal? CompareAtPrice { get; init; }
    public int Stock { get; init; }
    public int? LowStockThreshold { get; init; }
    public string Status { get; init; } = string.Empty; 
    public string Visibility { get; init; } = string.Empty;
    public CategoryDto Category { get; init; } = null!;
    public CategoryDto? SubCategory { get; init; }
    public List<ProductImageDto> Images { get; init; } = new();
    public List<ProductVariantDto> Variants { get; init; } = new();
    public List<ProductAttributeDto> Attributes { get; init; } = new();
    public ProductSeoDto? Seo { get; init; }
    public ProductDimensionsDto? Dimensions { get; init; }
    public List<string> Tags { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
    public bool IsArchived { get; init; }
}