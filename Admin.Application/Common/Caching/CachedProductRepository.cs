using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace Admin.Application.Common.Caching;
//CachedProductRepository.cs - Decorator pattern implementation
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _innerRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedProductRepository> _logger;

    private const string ProductKeyPrefix = "product:";
    private const string ProductListKey = "product:list";
    private const string VariantKeyPrefix = "variant:";

    public CachedProductRepository(
        IProductRepository innerRepository,
        ICacheService cache,
        ILogger<CachedProductRepository> logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}{id}";

        var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var product = await _innerRepository.GetByIdAsync(id, cancellationToken);
        if (product != null)
            await _cache.SetAsync(cacheKey, product, cancellationToken: cancellationToken);

        return product;
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}slug:{slug}";

        var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var product = await _innerRepository.GetBySlugAsync(slug, cancellationToken);
        if (product != null)
            await _cache.SetAsync(cacheKey, product, cancellationToken: cancellationToken);

        return product;
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{VariantKeyPrefix}{variantId}";

        var cached = await _cache.GetAsync<ProductVariant>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var variant = await _innerRepository.GetVariantByIdAsync(variantId, cancellationToken);
        if (variant != null)
            await _cache.SetAsync(cacheKey, variant, cancellationToken: cancellationToken);

        return variant;
    }

    public async Task<IEnumerable<ProductVariant>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Add(Product product)
    {
        _innerRepository.Add(product);
    }

    public void Update(Product product)
    {
        _innerRepository.Update(product);
        // Invalidate cache
        Task.Run(async () =>
        {
            await _cache.RemoveAsync($"{ProductKeyPrefix}{product.Id}");
            await _cache.RemoveAsync($"{ProductKeyPrefix}slug:{product.Slug}");
            await _cache.RemoveAsync(ProductListKey);
        });
    }

    public void Remove(Product product)
    {
        _innerRepository.Remove(product);
        // Invalidate cache
        Task.Run(async () =>
        {
            await _cache.RemoveAsync($"{ProductKeyPrefix}{product.Id}");
            await _cache.RemoveAsync($"{ProductKeyPrefix}slug:{product.Slug}");
            await _cache.RemoveAsync(ProductListKey);
            await _cache.RemoveByPrefixAsync($"{VariantKeyPrefix}");
        });
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        ProductFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        // For list/search operations, we'll cache only simple queries
        if (string.IsNullOrEmpty(filter.SearchTerm) &&
            !filter.CategoryId.HasValue &&
            !filter.SubCategoryId.HasValue &&
            !filter.MinPrice.HasValue &&
            !filter.MaxPrice.HasValue &&
            !filter.InStock.HasValue)
        {
            var cacheKey = $"{ProductListKey}:{filter.PageNumber}:{filter.PageSize}";
            var cached = await _cache.GetAsync<CacheProductListResult>(cacheKey, cancellationToken);
            if (cached != null)
                return (cached.Products, cached.TotalCount);
        }

        // If not cached or complex query, get from repository
        return await _innerRepository.GetProductsAsync(filter, cancellationToken);
    }

    private class CacheProductListResult
    {
        public IEnumerable<Product> Products { get; set; }
        public int TotalCount { get; set; }
    }
}
