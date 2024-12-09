namespace Admin.WebAPI.Endpoints.Products.ProductsVariant;

public record GetProductVariantRequest
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}
