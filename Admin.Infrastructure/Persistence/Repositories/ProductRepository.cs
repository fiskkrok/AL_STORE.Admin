//using Admin.Application.Common.Interfaces;
//using Admin.Application.Products.Queries;
//using Admin.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Hybrid;
//using Microsoft.Extensions.Logging;

//namespace Admin.Infrastructure.Persistence.Repositories;

//public class ProductRepository : IProductRepository
//{
//    private readonly AdminDbContext _context;
//    private readonly HybridCache _cache;
//    private readonly ILogger<ProductRepository> _logger;

//    private const string PRODUCT_PREFIX = "product:";
//    private const string PRODUCT_LIST_PREFIX = "product:list:";
//    private static readonly TimeSpan DEFAULT_CACHE_DURATION = TimeSpan.FromMinutes(30);

//    public ProductRepository(
//        AdminDbContext context,
//        HybridCache cache,
//        ILogger<ProductRepository> logger)
//    {
//        _context = context;
//        _cache = cache;
//        _logger = logger;
//    }

//    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
//    {
//        var cacheKey = $"{PRODUCT_PREFIX}{id}";

//        try
//        {
//            return await _cache.GetOrCreateAsync(
//                cacheKey,
//                async entry =>
//                {
//                    entry.SetOptions(new HybridCacheEntryOptions
//                    {
//                        Expiration = DEFAULT_CACHE_DURATION,
//                        LocalCacheExpiration = TimeSpan.FromMinutes(5), // Local cache expires faster
//                        Tags = new[] { "products", id.ToString() }
//                    });

//                    return await _context.Products
//                        .Include(p => p.Category)
//                        .Include(p => p.SubCategory)
//                        .Include(p => p.Images)
//                        .Include(p => p.Variants)
//                        .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived, cancellationToken);
//                }, cancellationToken: cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error retrieving product {ProductId} from cache", id);
//            // Fallback to database on cache error
//            return await _context.Products
//                .Include(p => p.Category)
//                .Include(p => p.SubCategory)
//                .Include(p => p.Images)
//                .Include(p => p.Variants)
//                .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived, cancellationToken);
//        }
//    }

//    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
//        ProductFilterRequest filter,
//        CancellationToken cancellationToken = default)
//    {
//        // Only cache simple queries
//        var shouldCache = string.IsNullOrEmpty(filter.SearchTerm) &&
//                          !filter.CategoryId.HasValue &&
//                          !filter.SubCategoryId.HasValue &&
//                          !filter.MinPrice.HasValue &&
//                          !filter.MaxPrice.HasValue &&
//                          !filter.InStock.HasValue;

//        if (!shouldCache)
//        {
//            return await GetProductsFromDb(filter, cancellationToken);
//        }

//        var cacheKey = $"{PRODUCT_LIST_PREFIX}{filter.PageNumber}:{filter.PageSize}:{filter.SortBy}:{filter.SortDescending}";

//        try
//        {
//            var result = await _cache.GetOrCreateAsync(
//                cacheKey,
//                async entry =>
//                {
//                    entry.SetOptions(new HybridCacheEntryOptions
//                    {
//                        Expiration = TimeSpan.FromMinutes(15),
//                        LocalCacheExpiration = TimeSpan.FromMinutes(5),
//                        Tags = new[] { "products", "product-list" }
//                    });

//                    return await GetProductsFromDb(filter, cancellationToken);
//                },
//                cancellationToken);

//            return result;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error retrieving product list from cache");
//            return await GetProductsFromDb(filter, cancellationToken);
//        }
//    }

//    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    private async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsFromDb(
//        ProductFilterRequest filter,
//        CancellationToken cancellationToken)
//    {
//        var query = _context.Products
//            .Include(p => p.Category)
//            .Include(p => p.SubCategory)
//            .Include(p => p.Images)
//            .Include(p => p.Variants)
//            .Where(p => !p.IsArchived)
//            .AsQueryable();

//        // Apply filters
//        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
//        {
//            var searchTerm = filter.SearchTerm.ToLower();
//            query = query.Where(p =>
//                EF.Functions.Like(p.Name.ToLower(), $"%{searchTerm}%") ||
//                EF.Functions.Like(p.Description.ToLower(), $"%{searchTerm}%") ||
//                EF.Functions.Like(p.Sku.ToLower(), $"%{searchTerm}%"));
//        }

//        if (filter.CategoryId.HasValue)
//            query = query.Where(p => p.CategoryId == filter.CategoryId);

//        if (filter.SubCategoryId.HasValue)
//            query = query.Where(p => p.SubCategoryId == filter.SubCategoryId);

//        if (filter.MinPrice.HasValue)
//            query = query.Where(p => p.Price.Amount >= filter.MinPrice);

//        if (filter.MaxPrice.HasValue)
//            query = query.Where(p => p.Price.Amount <= filter.MaxPrice);

//        if (filter.InStock.HasValue)
//            query = query.Where(p => filter.InStock.Value ? p.Stock > 0 : p.Stock == 0);

//        // Apply sorting
//        query = filter.SortBy?.ToLower() switch
//        {
//            "name" => filter.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
//            "price" => filter.SortDescending ? query.OrderByDescending(p => p.Price.Amount) : query.OrderBy(p => p.Price.Amount),
//            "created" => filter.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
//            _ => query.OrderByDescending(p => p.CreatedAt)
//        };

//        var totalCount = await query.CountAsync(cancellationToken);

//        var products = await query
//            .Skip((filter.PageNumber - 1) * filter.PageSize)
//            .Take(filter.PageSize)
//            .ToListAsync(cancellationToken);

//        return (products, totalCount);
//    }

//    public void Add(Product product)
//    {
//        _context.Products.Add(product);
//    }

//    public void Update(Product product)
//    {
//        _context.Products.Update(product);
//        // Invalidate cache
//        InvalidateProductCache(product.Id);
//    }

//    public void Remove(Product product)
//    {
//        _context.Products.Remove(product);
//        // Invalidate cache
//        InvalidateProductCache(product.Id);
//    }

//    private void InvalidateProductCache(Guid productId)
//    {
//        try
//        {
//            // Remove by tags to invalidate all related cache entries
//            _cache.RemoveByTag(productId.ToString());
//            _cache.RemoveByTag("product-list");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error invalidating cache for product {ProductId}", productId);
//        }
//    }
//}

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
