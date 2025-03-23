

using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Domain.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Admin.Infrastructure.Persistence.Decorators;
public class CachingProductRepositoryDecorator : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger _logger;
    private readonly TimeSpan _cacheExpiration;

    private const string ProductKeyPrefix = "product";
    private const string ProductSlugKeyPrefix = "product:slug";
    private const string VariantKeyPrefix = "variant";
    private const string ProductsListKeyPrefix = "products:list";

    public CachingProductRepositoryDecorator(
        IProductRepository inner,
        ICacheService cache,
        ILogger logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
    }

    // Implement IRepository<Product> methods with caching
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product with ID {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for product with ID {Id}", id);
            var product = await _inner.GetByIdAsync(id, cancellationToken);

            if (product != null)
            {
                await _cache.SetAsync(cacheKey, product, _cacheExpiration, cancellationToken);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product with ID {Id}, falling back to repository", id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    // Implement other specialized methods with caching
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductSlugKeyPrefix}:{slug}";

        try
        {
            var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product slug {Slug}", slug);
                return cached;
            }

            _logger.LogDebug("Cache miss for product slug {Slug}", slug);
            var product = await _inner.GetBySlugAsync(slug, cancellationToken);

            if (product != null)
            {
                await _cache.SetAsync(cacheKey, product, _cacheExpiration, cancellationToken);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product slug {Slug}, falling back to repository", slug);
            return await _inner.GetBySlugAsync(slug, cancellationToken);
        }
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{VariantKeyPrefix}:{variantId}";

        try
        {
            var cached = await _cache.GetAsync<ProductVariant>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for variant with ID {VariantId}", variantId);
                return cached;
            }

            _logger.LogDebug("Cache miss for variant with ID {VariantId}", variantId);
            var variant = await _inner.GetVariantByIdAsync(variantId, cancellationToken);

            if (variant != null)
            {
                await _cache.SetAsync(cacheKey, variant, _cacheExpiration, cancellationToken);
            }

            return variant;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for variant with ID {VariantId}, falling back to repository", variantId);
            return await _inner.GetVariantByIdAsync(variantId, cancellationToken);
        }
    }

    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}:{productId}:variants";

        try
        {
            var cached = await _cache.GetAsync<IEnumerable<ProductVariant>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for variants of product with ID {ProductId}", productId);
                return cached;
            }

            _logger.LogDebug("Cache miss for variants of product with ID {ProductId}", productId);
            var variants = await _inner.GetVariantsByProductIdAsync(productId, cancellationToken);

            if (variants != null)
            {
                await _cache.SetAsync(cacheKey, variants, _cacheExpiration, cancellationToken);
            }

            return variants;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for variants of product with ID {ProductId}, falling back to repository", productId);
            return await _inner.GetVariantsByProductIdAsync(productId, cancellationToken);
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductSlugKeyPrefix}:exists:{slug}:{excludeProductId}";

        try
        {
            var cached = await _cache.GetAsync<string>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for slug existence check for slug {Slug}", slug);
                return bool.Parse(cached);
            }

            _logger.LogDebug("Cache miss for slug existence check for slug {Slug}", slug);
            var exists = await _inner.SlugExistsAsync(slug, excludeProductId, cancellationToken);

            await _cache.SetAsync(cacheKey, exists.ToString(), _cacheExpiration, cancellationToken);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for slug existence check for slug {Slug}, falling back to repository", slug);
            return await _inner.SlugExistsAsync(slug, excludeProductId, cancellationToken);
        }
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(ProductFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductsListKeyPrefix}:{filter}";

        try
        {
            var cached = await _cache.GetAsync<string>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product list with filter {Filter}", filter);
                return JsonConvert.DeserializeObject<(IEnumerable<Product> Products, int TotalCount)>(cached);
            }

            _logger.LogDebug("Cache miss for product list with filter {Filter}", filter);
            var products = await _inner.GetProductsAsync(filter, cancellationToken);

            await _cache.SetAsync(cacheKey, JsonConvert.SerializeObject(products), _cacheExpiration, cancellationToken);

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product list with filter {Filter}, falling back to repository", filter);
            return await _inner.GetProductsAsync(filter, cancellationToken);
        }
    }


    // For methods that need to invalidate cached items
    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        await InvalidateProductCacheAsync(entity, cancellationToken);
    }

    public async Task RemoveAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        await InvalidateProductCacheAsync(entity, cancellationToken);
    }

    private async Task InvalidateProductCacheAsync(Product product, CancellationToken cancellationToken)
    {
        try
        {
            var keysToInvalidate = new List<string>
            {
                $"{ProductKeyPrefix}:{product.Id}",
                $"{ProductSlugKeyPrefix}:{product.Slug}"
            };

            // Invalidate variants cache if applicable
            if (product.Variants != null)
            {
                foreach (var variant in product.Variants)
                {
                    keysToInvalidate.Add($"{VariantKeyPrefix}:{variant.Id}");
                }
            }

            // Invalidate list caches
            await _cache.RemoveByPrefixAsync(ProductsListKeyPrefix, cancellationToken);

            // Remove individual keys
            foreach (var key in keysToInvalidate)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Invalidated cache key {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for product {ProductId}", product.Id);
        }
    }
}
