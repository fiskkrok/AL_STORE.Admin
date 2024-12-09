namespace Admin.WebAPI.Hubs.Models;

public class UserConnection
{
    public string? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

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

public class ProductStatusUpdate
{
    public Guid ProductId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}