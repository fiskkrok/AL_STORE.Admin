using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Domain.Entities;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Persistence.Repositories;

public class CacheSettings
{
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan ExtendedExpiration { get; set; } = TimeSpan.FromHours(24);
    public int MaxCacheSize { get; set; } = 1000;
}

public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _innerRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedProductRepository> _logger;
    private readonly CacheSettings _settings;

    private const string ProductKeyPrefix = "product:";
    private const string ProductListKeyPrefix = "products:list:";
    private const string ProductSlugKeyPrefix = "product:slug:";
    private const string VariantKeyPrefix = "variant:";

    public CachedProductRepository(
        IProductRepository innerRepository,
        ICacheService cache,
        IOptions<CacheSettings> settings,
        ILogger<CachedProductRepository> logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{ProductKeyPrefix}{id}";

            var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product {ProductId}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for product {ProductId}", id);
            var product = await _innerRepository.GetByIdAsync(id, cancellationToken);

            if (product != null)
            {
                await _cache.SetAsync(
                    cacheKey,
                    product,
                    _settings.DefaultExpiration,
                    cancellationToken);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId} from cache", id);
            return await _innerRepository.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{ProductSlugKeyPrefix}{slug}";

            var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product slug {Slug}", slug);
                return cached;
            }

            _logger.LogDebug("Cache miss for product slug {Slug}", slug);
            var product = await _innerRepository.GetBySlugAsync(slug, cancellationToken);

            if (product != null)
            {
                await _cache.SetAsync(
                    cacheKey,
                    product,
                    _settings.DefaultExpiration,
                    cancellationToken);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by slug {Slug} from cache", slug);
            return await _innerRepository.GetBySlugAsync(slug, cancellationToken);
        }
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{VariantKeyPrefix}{variantId}";

            var cached = await _cache.GetAsync<ProductVariant>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for variant {VariantId}", variantId);
                return cached;
            }

            _logger.LogDebug("Cache miss for variant {VariantId}", variantId);
            var variant = await _innerRepository.GetVariantByIdAsync(variantId, cancellationToken);

            if (variant != null)
            {
                await _cache.SetAsync(
                    cacheKey,
                    variant,
                    _settings.DefaultExpiration,
                    cancellationToken);
            }

            return variant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variant {VariantId} from cache", variantId);
            return await _innerRepository.GetVariantByIdAsync(variantId, cancellationToken);
        }
    }

    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{ProductKeyPrefix}{productId}:variants";

            var cached = await _cache.GetAsync<IEnumerable<ProductVariant>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product variants {ProductId}", productId);
                return cached;
            }

            _logger.LogDebug("Cache miss for product variants {ProductId}", productId);
            var variants = await _innerRepository.GetVariantsByProductIdAsync(productId, cancellationToken);

            await _cache.SetAsync(
                cacheKey,
                variants,
                _settings.DefaultExpiration,
                cancellationToken);

            return variants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variants for product {ProductId} from cache", productId);
            return await _innerRepository.GetVariantsByProductIdAsync(productId, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        // Only cache simple queries
        if (IsCacheableQuery(filter))
        {
            try
            {
                var cacheKey = BuildListCacheKey(filter);
                var cached = await _cache.GetAsync<CachedProductList>(cacheKey, cancellationToken);

                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for product list with key {CacheKey}", cacheKey);
                    return (cached.Products, cached.TotalCount);
                }

                _logger.LogDebug("Cache miss for product list with key {CacheKey}", cacheKey);
                var (products, totalCount) = await _innerRepository.GetProductsAsync(filter, cancellationToken);

                var cacheItem = new CachedProductList
                {
                    Products = products.ToList(),
                    TotalCount = totalCount
                };

                await _cache.SetAsync(
                    cacheKey,
                    cacheItem,
                    _settings.DefaultExpiration,
                    cancellationToken);

                return (products, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product list from cache");
            }
        }

        return await _innerRepository.GetProductsAsync(filter, cancellationToken);
    }

    // Implement new async methods from IRepository<Product>
    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(product, cancellationToken);
        _logger.LogInformation("Added new product {ProductId}", product.Id);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(product, cancellationToken);
        await InvalidateProductCacheAsync(product, cancellationToken);
        _logger.LogInformation("Updated product {ProductId}", product.Id);
    }

    public async Task RemoveAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _innerRepository.RemoveAsync(product, cancellationToken);
        await InvalidateProductCacheAsync(product, cancellationToken);
        _logger.LogInformation("Removed product {ProductId}", product.Id);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        // Don't cache slug existence checks as they're typically only used during creation/update
        return await _innerRepository.SlugExistsAsync(slug, excludeProductId, cancellationToken);
    }

    private async Task InvalidateProductCacheAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting cache invalidation for product {ProductId}", product.Id);

            var keysToInvalidate = new List<string>
            {
                $"{ProductKeyPrefix}{product.Id}",
                $"{ProductSlugKeyPrefix}{product.Slug}",
                $"{ProductKeyPrefix}{product.Id}:variants"
            };

            // Add variant-specific keys
            if (product.Variants != null)
            {
                foreach (var variant in product.Variants)
                {
                    keysToInvalidate.Add($"{VariantKeyPrefix}{variant.Id}");
                }
            }

            // Batch the invalidation tasks
            var tasks = keysToInvalidate.Select(key =>
                _cache.RemoveAsync(key, cancellationToken)).ToList();

            // Also invalidate list caches
            tasks.Add(_cache.RemoveByPrefixAsync(ProductListKeyPrefix, cancellationToken));

            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "Successfully invalidated {Count} cache entries for product {ProductId}",
                keysToInvalidate.Count,
                product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for product {ProductId}. Cache may be in inconsistent state.",
                product.Id);
            throw;
        }
    }

    private static bool IsCacheableQuery(ProductFilterRequest filter)
    {
        return string.IsNullOrEmpty(filter.SearchTerm) &&
               !filter.CategoryId.HasValue &&
               !filter.SubCategoryId.HasValue &&
               !filter.MinPrice.HasValue &&
               !filter.MaxPrice.HasValue &&
               !filter.InStock.HasValue;
    }

    private static string BuildListCacheKey(ProductFilterRequest filter)
    {
        return $"{ProductListKeyPrefix}{filter.PageNumber}:{filter.PageSize}:{filter.SortBy}:{filter.SortDescending}";
    }

    private class CachedProductList
    {
        public List<Product> Products { get; set; } = new();
        public int TotalCount { get; set; }
    }
}