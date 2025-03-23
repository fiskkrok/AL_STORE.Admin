using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;
public class CachingCategoryRepositoryDecorator : ICategoryRepository
{
    private readonly ICategoryRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger _logger;
    private readonly TimeSpan _cacheExpiration;

    private const string CategoryKeyPrefix = "category:id";
    private const string CategoriesListKey = "categories:all";

    public CachingCategoryRepositoryDecorator(
        ICategoryRepository inner,
        ICacheService cache,
        ILogger logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CategoryKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<Category>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for category with ID {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for category with ID {Id}", id);
            var category = await _inner.GetByIdAsync(id, cancellationToken);

            if (category != null)
            {
                await _cache.SetAsync(cacheKey, category, _cacheExpiration, cancellationToken);
            }

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for category with ID {Id}, falling back to repository", id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CategoriesListKey}:{includeInactive}";

        try
        {
            var cached = await _cache.GetAsync<IEnumerable<Category>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for all categories (includeInactive: {IncludeInactive})", includeInactive);
                return cached;
            }

            _logger.LogDebug("Cache miss for all categories (includeInactive: {IncludeInactive})", includeInactive);
            var categories = await _inner.GetAllAsync(includeInactive, cancellationToken);

            await _cache.SetAsync(cacheKey, categories, _cacheExpiration, cancellationToken);

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for all categories, falling back to repository");
            return await _inner.GetAllAsync(includeInactive, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // For simple existence checks, we can optimize by checking if we have it cached first
        var cacheKey = $"{CategoryKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<Category>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return true; // If it's in cache, it exists
            }
        }
        catch (Exception)
        {
            // Ignore cache errors for exists check
        }

        // Fall back to repository
        return await _inner.ExistsAsync(id, cancellationToken);
    }

    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        await InvalidateCategoryCacheAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        await InvalidateCategoryCacheAsync(entity.Id, cancellationToken);
    }

    public async Task RemoveAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        await InvalidateCategoryCacheAsync(entity.Id, cancellationToken);
    }

    private async Task InvalidateCategoryCacheAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            // Invalidate specific category
            await _cache.RemoveAsync($"{CategoryKeyPrefix}:{id}", cancellationToken);

            // Invalidate category list
            await InvalidateCategoryCacheAsync(cancellationToken);

            _logger.LogDebug("Invalidated category cache for ID {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating category cache for ID {Id}", id);
        }
    }

    private async Task InvalidateCategoryCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Invalidate all categories list (both active and including inactive)
            await _cache.RemoveAsync($"{CategoriesListKey}:True", cancellationToken);
            await _cache.RemoveAsync($"{CategoriesListKey}:False", cancellationToken);

            _logger.LogDebug("Invalidated all categories cache");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating all categories cache");
        }
    }
}
