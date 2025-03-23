namespace Admin.WebAPI.Endpoints.ProductsVariant.Models;

public record GetProductVariantRequest
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}
