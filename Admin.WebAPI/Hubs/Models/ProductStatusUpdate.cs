namespace Admin.WebAPI.Hubs.Models;

public class ProductStatusUpdate
{
    public Guid ProductId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}