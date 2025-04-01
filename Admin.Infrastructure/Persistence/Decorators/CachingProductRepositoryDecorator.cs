using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Domain.Entities;
using Admin.Infrastructure.Services.Caching.DTOs;

using AutoMapper;

using Microsoft.Extensions.Logging;

/// <summary>
/// Enhanced caching decorator for ProductRepository that uses specialized cache DTOs
/// </summary>
public class CachingProductRepositoryDecorator : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CachingProductRepositoryDecorator> _logger;
    private readonly TimeSpan _cacheExpiration;

    // Cache key prefixes
    private const string ProductKeyPrefix = "product:dto";
    private const string ProductSlugKeyPrefix = "product:slug:dto";
    private const string VariantKeyPrefix = "variant:dto";
    private const string ProductsListKeyPrefix = "products:list:dto";

    public CachingProductRepositoryDecorator(
        IProductRepository inner,
        ICacheService cache,
        IMapper mapper,
        ILogger<CachingProductRepositoryDecorator> logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
    }

    public class GuidWrapper
    {
        public Guid Value { get; set; }
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}:{id}";

        try
        {
            // Try to get from cache first
            var cachedDto = await _cache.GetAsync<ProductCacheDto>(cacheKey, cancellationToken);
            if (cachedDto != null)
            {
                _logger.LogDebug("Cache hit for product with ID {Id}", id);

                // Map back to domain entity with minimal hydration
                var map = _mapper.Map<Product>(cachedDto);

                // For full entity with all relationships, we need to get from repository
                // This is a trade-off between full caching and partial caching
                if (map.Category == null || map.Variants.Count != cachedDto.Variants.Count)
                {
                    _logger.LogDebug("Cache hit for product {Id} but needs relationship hydration", id);
                    return await _inner.GetByIdAsync(id, cancellationToken);
                }

                return map;
            }

            _logger.LogDebug("Cache miss for product with ID {Id}", id);

            // Get from repository
            var product = await _inner.GetByIdAsync(id, cancellationToken);

            if (product != null)
            {
                // Convert to cache DTO and store
                var productDto = _mapper.Map<ProductCacheDto>(product);
                await _cache.SetAsync(cacheKey, productDto, _cacheExpiration, cancellationToken);

                // Also cache by slug for slug lookups
                await _cache.SetAsync($"{ProductSlugKeyPrefix}:{product.Slug}",
                    new GuidWrapper { Value = product.Id }, _cacheExpiration, cancellationToken);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product with ID {Id}, falling back to repository", id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductSlugKeyPrefix}:{slug}";

        try
        {
            // Check if we have the product ID cached by slug
            var cachedGuidWrapper = await _cache.GetAsync<GuidWrapper>(cacheKey, cancellationToken);

            if (cachedGuidWrapper != null)
            {
                // If we have the ID, try to get the product by ID (which might also be cached)
                var byIdAsync = await GetByIdAsync(cachedGuidWrapper.Value, cancellationToken);
                if (byIdAsync != null)
                {
                    return byIdAsync;
                }
            }

            // Cache miss or product not found by ID, get from repository
            var product = await _inner.GetBySlugAsync(slug, cancellationToken);

            if (product != null)
            {
                // Cache the ID-by-slug mapping (lightweight)
                await _cache.SetAsync(cacheKey, new GuidWrapper { Value = product.Id }, _cacheExpiration, cancellationToken);

                // Also cache the full product
                var productDto = _mapper.Map<ProductCacheDto>(product);
                await _cache.SetAsync($"{ProductKeyPrefix}:{product.Id}",
                    productDto, _cacheExpiration, cancellationToken);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product slug {Slug}, falling back to repository", slug);
            return await _inner.GetBySlugAsync(slug, cancellationToken);
        }
    }

    public async Task<Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken)
    {
        // For methods requiring specific relationships, we go directly to repository
        // Don't try to cache these as they're likely used in specific write scenarios
        return await _inner.GetByIdWithImagesAsync(id, cancellationToken);
    }

    

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{VariantKeyPrefix}:{variantId}";

        try
        {
            // Try to get from cache first
            var cachedDto = await _cache.GetAsync<ProductVariantCacheDto>(cacheKey, cancellationToken);
            if (cachedDto != null)
            {
                _logger.LogDebug("Cache hit for variant with ID {Id}", variantId);

                // Map back to domain entity
                var map = _mapper.Map<ProductVariant>(cachedDto);
                return map;
            }

            _logger.LogDebug("Cache miss for variant with ID {Id}", variantId);

            // Get from repository
            var variant = await _inner.GetVariantByIdAsync(variantId, cancellationToken);

            if (variant != null)
            {
                // Convert to cache DTO and store
                var variantDto = _mapper.Map<ProductVariantCacheDto>(variant);
                await _cache.SetAsync(cacheKey, variantDto, _cacheExpiration, cancellationToken);
            }

            return variant;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for variant with ID {Id}, falling back to repository", variantId);
            return await _inner.GetVariantByIdAsync(variantId, cancellationToken);
        }
    }

    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}:{productId}:variants";

        try
        {
            // Try to get from cache
            var cachedDtos = await _cache.GetAsync<List<ProductVariantCacheDto>>(cacheKey, cancellationToken);
            if (cachedDtos != null)
            {
                _logger.LogDebug("Cache hit for variants of product {ProductId}", productId);

                // Map back to domain entities
                var map = _mapper.Map<List<ProductVariant>>(cachedDtos);
                return map;
            }

            // Get from repository
            var variants = await _inner.GetVariantsByProductIdAsync(productId, cancellationToken);

            // Cache the variants
            if (variants != null && variants.Any())
            {
                var variantDtos = _mapper.Map<List<ProductVariantCacheDto>>(variants);
                await _cache.SetAsync(cacheKey, variantDtos, _cacheExpiration, cancellationToken);

                // Also cache individual variants
                foreach (var variant in variants)
                {
                    var variantDto = _mapper.Map<ProductVariantCacheDto>(variant);
                    await _cache.SetAsync($"{VariantKeyPrefix}:{variant.Id}",
                        variantDto, _cacheExpiration, cancellationToken);
                }
            }

            return variants;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for variants of product {ProductId}, falling back to repository", productId);
            return await _inner.GetVariantsByProductIdAsync(productId, cancellationToken);
        }
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        // For simple existence checks, it's usually more efficient to go directly to database
        return await _inner.SlugExistsAsync(slug, excludeProductId, cancellationToken);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter, CancellationToken cancellationToken = default)
    {
        // For filtered lists, we need a way to represent the filter in the cache key
        var filterHash = ComputeFilterHash(filter);
        var cacheKey = $"{ProductsListKeyPrefix}:{filterHash}";

        try
        {
            // Try to get product IDs from cache first (lightweight approach)
            var cachedResult = await _cache.GetAsync<ProductListCacheResult>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogDebug("Cache hit for product list with filter hash {FilterHash}", filterHash);

                // For product lists, we just cache IDs and total count to avoid massive objects
                if (cachedResult.ProductIds.Count > 0)
                {
                    var products = await GetProductsByIdsAsync(cachedResult.ProductIds, cancellationToken);
                    return (products, cachedResult.TotalCount);
                }
            }

            // Get from repository
            var result = await _inner.GetProductsAsync(filter, cancellationToken);

            // Cache product IDs and total count
            if (result.Products.Any())
            {
                var productIds = result.Products.Select(p => p.Id).ToList();
                var cacheResult = new ProductListCacheResult
                {
                    ProductIds = productIds,
                    TotalCount = result.TotalCount
                };

                await _cache.SetAsync(cacheKey, cacheResult, _cacheExpiration, cancellationToken);

                // Also cache individual products
                foreach (var product in result.Products)
                {
                    var productDto = _mapper.Map<ProductCacheDto>(product);
                    await _cache.SetAsync($"{ProductKeyPrefix}:{product.Id}",
                        productDto, _cacheExpiration, cancellationToken);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product list, falling back to repository");
            return await _inner.GetProductsAsync(filter, cancellationToken);
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var result = new List<Product>();
        var missingProductIds = new List<Guid>();

        // Try to get each product from cache
        foreach (var productId in productIds)
        {
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                result.Add(product);
            }
            else
            {
                missingProductIds.Add(productId);
            }
        }

        // For any products not in cache, get them from repository
        if (missingProductIds.Any())
        {
            var products = await _inner.GetProductsByIdsAsync(missingProductIds, cancellationToken);
            result.AddRange(products);

            // Cache these products
            foreach (var product in products)
            {
                var productDto = _mapper.Map<ProductCacheDto>(product);
                await _cache.SetAsync($"{ProductKeyPrefix}:{product.Id}",
                    productDto, _cacheExpiration, cancellationToken);
            }
        }

        return result;
    }

    // For write operations, we need to invalidate cache
    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        // No need to cache yet - entity ID might not be generated
    }

    public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        await InvalidateProductCacheAsync(entity, cancellationToken);

        // After update, refresh the cache with new entity
        var productDto = _mapper.Map<ProductCacheDto>(entity);
        await _cache.SetAsync($"{ProductKeyPrefix}:{entity.Id}",
            productDto, _cacheExpiration, cancellationToken);
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
                $"{ProductSlugKeyPrefix}:{product.Slug}",
                $"{ProductKeyPrefix}:{product.Id}:variants"
            };

            // Invalidate variants cache if applicable
            if (product.Variants != null)
            {
                foreach (var variant in product.Variants)
                {
                    keysToInvalidate.Add($"{VariantKeyPrefix}:{variant.Id}");
                }
            }

            // Invalidate product lists
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

    private string ComputeFilterHash(ProductFilterRequest filter)
    {
        // Create a deterministic string representation of the filter for use as a cache key
        return $"pg{filter.PageNumber}sz{filter.PageSize}" +
               $"st{filter.SearchTerm}" +
               $"c{filter.CategoryId}" +
               $"sc{filter.SubCategoryId}" +
               $"p{filter.MinPrice}-{filter.MaxPrice}" +
               $"i{filter.InStock}" +
               $"s{filter.SortBy}-{filter.SortDescending}" +
               $"lm{filter.LastModifiedAfter?.Ticks}" +
               $"st{filter.Status}" +
               $"vs{filter.Visibility}" +
               $"ii{filter.IncludeInactive}";
    }
}

/// <summary>
/// Class for caching product list results
/// </summary>
public class ProductListCacheResult
{
    public List<Guid> ProductIds { get; set; } = new();
    public int TotalCount { get; set; }
}