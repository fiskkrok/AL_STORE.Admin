using Admin.Application.Products.Queries;
using Admin.Domain.Entities;

namespace Admin.Application.Common.Interfaces;
public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<Product?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<ProductVariant?> GetVariantByIdAsync(
        Guid variantId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default);
}