
namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record ProductAttributeRequest
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}