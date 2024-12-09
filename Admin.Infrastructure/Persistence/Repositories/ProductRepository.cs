using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly AdminDbContext _context;

    public ProductRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Attributes)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsArchived, cancellationToken);
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ProductVariant>()
            .Include(v => v.Attributes)
            .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
    }

    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ProductVariant>()
            .Include(v => v.Attributes)
            .Where(v => v.ProductId == productId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.Where(p => p.Slug == slug && !p.IsArchived);

        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => !p.IsArchived)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm) ||
                p.ShortDescription.ToLower().Contains(searchTerm) ||
                p.Sku.ToLower().Contains(searchTerm));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId);

        if (filter.SubCategoryId.HasValue)
            query = query.Where(p => p.SubCategoryId == filter.SubCategoryId);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price.Amount >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price.Amount <= filter.MaxPrice);

        if (filter.InStock.HasValue)
            query = query.Where(p => filter.InStock.Value ? p.Stock > 0 : p.Stock == 0);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(p => p.Status.ToString() == filter.Status);

        if (!string.IsNullOrEmpty(filter.Visibility))
            query = query.Where(p => p.Visibility.ToString() == filter.Visibility);

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortDescending ?
                query.OrderByDescending(p => p.Name) :
                query.OrderBy(p => p.Name),
            "price" => filter.SortDescending ?
                query.OrderByDescending(p => p.Price.Amount) :
                query.OrderBy(p => p.Price.Amount),
            "stock" => filter.SortDescending ?
                query.OrderByDescending(p => p.Stock) :
                query.OrderBy(p => p.Stock),
            "status" => filter.SortDescending ?
                query.OrderByDescending(p => p.Status) :
                query.OrderBy(p => p.Status),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }
}
