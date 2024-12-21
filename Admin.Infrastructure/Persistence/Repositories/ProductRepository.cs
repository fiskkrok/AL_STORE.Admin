using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(
        AdminDbContext context,
        ILogger<ProductRepository> logger)
        : base(context, logger)
    {
        _logger = logger;
    }

    // Override base GetByIdAsync to include related data
    public override async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching product with ID: {ProductId}", id);

            return await Context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Include(p => p.Attributes)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product with ID: {ProductId}", id);
            throw;
        }
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching product with slug: {Slug}", slug);

            return await Context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Include(p => p.Attributes)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsArchived, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product with slug: {Slug}", slug);
            throw;
        }
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching variant with ID: {VariantId}", variantId);

            return await Context.Set<ProductVariant>()
                .Include(v => v.Attributes)
                .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching variant with ID: {VariantId}", variantId);
            throw;
        }
    }

    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching variants for product ID: {ProductId}", productId);

            return await Context.Set<ProductVariant>()
                .Include(v => v.Attributes)
                .Where(v => v.ProductId == productId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching variants for product ID: {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if slug exists: {Slug}", slug);

            var query = Context.Products
                .Where(p => p.Slug == slug && !p.IsArchived);

            if (excludeProductId.HasValue)
                query = query.Where(p => p.Id != excludeProductId.Value);

            return await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking slug existence: {Slug}", slug);
            throw;
        }
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching products with filter: {@Filter}", filter);

            var query = Context.Products
                .AsTracking() // Add this explicitly
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => !p.IsArchived)
                .AsSplitQuery()
                .AsQueryable();

            // Add logging here to see raw SQL
            _logger.LogInformation("Raw SQL: {Sql}", query.ToQueryString());

            query = ApplyFilters(query, filter);

            var totalCount = await query.CountAsync(cancellationToken);

            query = ApplySorting(query, filter);

            var products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (products, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products with filter: {@Filter}", filter);
            throw;
        }
    }

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterRequest filter)
    {
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

        return query;
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductFilterRequest filter)
    {
        return filter.SortBy?.ToLower() switch
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
    }
}