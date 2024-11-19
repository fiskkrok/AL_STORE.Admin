using Admin.Application.Products.Queries;
using Admin.Domain.Entities;

namespace Admin.Application.Common.Interfaces;
public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter,
        CancellationToken cancellationToken = default);
}
