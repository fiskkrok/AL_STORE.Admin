namespace Admin.Infrastructure.Services.Caching.DTOs;

public class StockItemCacheDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }
    public int LowStockThreshold { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public int AvailableStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsActive { get; set; }
}