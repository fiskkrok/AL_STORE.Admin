namespace Admin.Infrastructure.Services.Caching.DTOs;

public class ProductVariantCacheDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool TrackInventory { get; set; }
    public Guid ProductId { get; set; }
}