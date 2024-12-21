namespace Admin.WebAPI.Endpoints.ProductsVariant;

public record GetProductVariantRequest
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
}
