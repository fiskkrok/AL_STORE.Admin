namespace Admin.WebAPI.Hubs.Models;

public class ProductPriceUpdate
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public decimal NewPrice { get; set; }
    public decimal? OldPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}