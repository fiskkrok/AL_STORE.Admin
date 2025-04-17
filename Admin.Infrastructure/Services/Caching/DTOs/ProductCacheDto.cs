namespace Admin.Infrastructure.Services.Caching.DTOs;

// DTOs specifically designed for caching

public class ProductCacheDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductTypeId { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid? SubCategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Lightweight collections, not including every relationship
    public List<ProductVariantCacheDto> Variants { get; set; } = new();
    public List<ProductImageCacheDto> Images { get; set; } = new();
}