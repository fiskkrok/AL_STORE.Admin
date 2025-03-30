namespace Admin.WebAPI.Endpoints.Statistics;

public class StatisticsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProductCount { get; set; }
    public int LowStockCount { get; set; }
    public int TotalCategories { get; set; }
    public decimal? TotalRevenue { get; set; }
    public decimal? RevenueChange { get; set; }
    public int? TotalOrders { get; set; }
    public decimal? OrdersChange { get; set; }
}