namespace Admin.WebAPI.Hubs.Models;

public class ProductInventoryUpdate
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int NewStock { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsLowStock { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}