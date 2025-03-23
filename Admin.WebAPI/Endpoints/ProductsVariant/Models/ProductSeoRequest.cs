namespace Admin.WebAPI.Endpoints.ProductsVariant.Models;

public record ProductSeoRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public List<string> Keywords { get; init; } = new();
}