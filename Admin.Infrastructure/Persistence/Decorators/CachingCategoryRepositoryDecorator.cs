using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Admin.Infrastructure.Services.Caching.DTOs;

using AutoMapper;

using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;

/// <summary>
/// Enhanced caching decorator for CategoryRepository that uses specialized cache DTOs
/// </summary>
public class CachingCategoryRepositoryDecorator : ICategoryRepository
{
    private readonly ICategoryRepository _inner;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CachingCategoryRepositoryDecorator> _logger;
    private readonly TimeSpan _cacheExpiration;

    private const string CategoryKeyPrefix = "category:dto";
    private const string CategoriesListKey = "categories:list:dto";
    private const string CategoryExistsKeyPrefix = "category:exists:dto";

    public CachingCategoryRepositoryDecorator(
        ICategoryRepository inner,
        ICacheService cache,
        IMapper mapper,
        ILogger<CachingCategoryRepositoryDecorator> logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CategoryKeyPrefix}:{id}";

        try
        {
            // Try to get from cache first
            var cachedDto = await _cache.GetAsync<CategoryCacheDto>(cacheKey, cancellationToken);
            if (cachedDto != null)
            {
                _logger.LogDebug("Cache hit for category with ID {Id}", id);

                // For read operations, we can map the cache DTO to a domain entity
                // Note: This won't have fully hydrated relationships
                var map = _mapper.Map<Category>(cachedDto);

                // If we need parent/child relationships, we'll need to hydrate those
                if (cachedDto.ParentCategoryId.HasValue)
                {
                    var parentCacheKey = $"{CategoryKeyPrefix}:{cachedDto.ParentCategoryId.Value}";
                    var parentCachedDto = await _cache.GetAsync<CategoryCacheDto>(parentCacheKey, cancellationToken);

                    if (parentCachedDto != null)
                    {
                        var parentCategory = _mapper.Map<Category>(parentCachedDto);
                        // We would need to use reflection to set the ParentCategory property
                        // since it doesn't have a public setter
                        SetParentCategory(map, parentCategory);
                    }
                }

                return map;
            }

            _logger.LogDebug("Cache miss for category with ID {Id}", id);

            // Get from repository
            var category = await _inner.GetByIdAsync(id, cancellationToken);

            if (category != null)
            {
                // Convert to cache DTO and store
                var categoryDto = _mapper.Map<CategoryCacheDto>(category);
                await _cache.SetAsync(cacheKey, categoryDto, _cacheExpiration, cancellationToken);
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
            // Try to get from cache first
            var cachedDtos = await _cache.GetAsync<List<CategoryCacheDto>>(cacheKey, cancellationToken);
            if (cachedDtos != null)
            {
                _logger.LogDebug("Cache hit for all categories (includeInactive: {IncludeInactive})", includeInactive);

                // Map DTOs to domain entities
                var map = _mapper.Map<List<Category>>(cachedDtos);

                // Reconstruct parent-child relationships
                ReconstructCategoryHierarchy(map, cachedDtos);

                return map;
            }

            _logger.LogDebug("Cache miss for all categories (includeInactive: {IncludeInactive})", includeInactive);

            // Get from repository
            var categories = await _inner.GetAllAsync(includeInactive, cancellationToken);
            var categoriesList = categories.ToList();

            // Convert to cache DTOs and store
            var categoryDtos = _mapper.Map<List<CategoryCacheDto>>(categoriesList);

            // Use a short expiration for the list to avoid stale data
            var listExpiration = TimeSpan.FromMinutes(15);
            await _cache.SetAsync(cacheKey, categoryDtos, listExpiration, cancellationToken);

            // Also cache individual categories with longer expiration
            foreach (var category in categoriesList)
            {
                var categoryDto = _mapper.Map<CategoryCacheDto>(category);
                var individualCacheKey = $"{CategoryKeyPrefix}:{category.Id}";
                await _cache.SetAsync(individualCacheKey, categoryDto, _cacheExpiration, cancellationToken);
            }

            return categoriesList;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for all categories, falling back to repository");
            return await _inner.GetAllAsync(includeInactive, cancellationToken);
        }
    }
    public class BoolWrapper
    {
        public bool Value { get; set; }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CategoryExistsKeyPrefix}:{id}";

        try
        {
            // Check if we have a cached result for existence check
            var cachedExists = await _cache.GetAsync<BoolWrapper>(cacheKey, cancellationToken);
            if (cachedExists != null)
            {
                _logger.LogDebug("Cache hit for category existence check {Id}", id);
                return cachedExists.Value;
            }

            // Check if we have the category cached
            var categoryCacheKey = $"{CategoryKeyPrefix}:{id}";
            var cachedCategory = await _cache.GetAsync<CategoryCacheDto>(categoryCacheKey, cancellationToken);
            if (cachedCategory != null)
            {
                await _cache.SetAsync(cacheKey, new BoolWrapper { Value = true }, _cacheExpiration, cancellationToken);
                return true;
            }

            // Fall back to repository
            _logger.LogDebug("Cache miss for category existence check {Id}", id);
            var exists = await _inner.ExistsAsync(id, cancellationToken);

            // Cache the result
            await _cache.SetAsync(cacheKey, new BoolWrapper { Value = exists }, _cacheExpiration, cancellationToken);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for category exists check {Id}, falling back to repository", id);
            return await _inner.ExistsAsync(id, cancellationToken);
        }
    }

    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);

        // After adding, we should invalidate the categories list cache
        // but we don't need to add the entity to cache yet since it might not have an ID
        try
        {
            await InvalidateCategoriesListCacheAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating categories list cache after adding category {CategoryId}", entity.Id);
        }
    }

    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);

        // After updating, we should invalidate both the specific category cache
        // and the categories list cache
        try
        {
            await InvalidateCategoryAsync(entity.Id, cancellationToken);
            await InvalidateCategoriesListCacheAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating category cache after updating category {CategoryId}", entity.Id);
        }
    }

    public async Task RemoveAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);

        // After removing, we should invalidate both the specific category cache
        // and the categories list cache
        try
        {
            await InvalidateCategoryAsync(entity.Id, cancellationToken);
            await InvalidateCategoriesListCacheAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating category cache after removing category {CategoryId}", entity.Id);
        }
    }

    #region Private Helper Methods

    private async Task InvalidateCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var keysToInvalidate = new[]
        {
            $"{CategoryKeyPrefix}:{id}",
            $"{CategoryExistsKeyPrefix}:{id}"
        };

        foreach (var key in keysToInvalidate)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Invalidated cache key {Key}", key);
        }
    }

    private async Task InvalidateCategoriesListCacheAsync(CancellationToken cancellationToken)
    {
        // Invalidate both variants (with and without inactive categories)
        var keysToInvalidate = new[]
        {
            $"{CategoriesListKey}:True",
            $"{CategoriesListKey}:False"
        };

        foreach (var key in keysToInvalidate)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Invalidated cache key {Key}", key);
        }
    }

    private void SetParentCategory(Category category, Category? parentCategory)
    {
        // Since ParentCategory doesn't have a public setter, we need to use reflection
        // This is a bit of a hack, but it's necessary for proper hydration
        var field = typeof(Category).GetField("_parentCategory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(category, parentCategory);
        }
    }

    private void ReconstructCategoryHierarchy(List<Category> categories, List<CategoryCacheDto> categoryDtos)
    {
        // Create a lookup for efficient retrieval
        var categoryLookup = categories.ToDictionary(c => c.Id);
        var dtosLookup = categoryDtos.ToDictionary(dto => dto.Id);

        // Reconstruct parent-child relationships
        foreach (var category in categories)
        {
            // Get the corresponding DTO
            if (dtosLookup.TryGetValue(category.Id, out var dto) && dto.ParentCategoryId.HasValue)
            {
                // If this category has a parent, set the parent reference
                if (categoryLookup.TryGetValue(dto.ParentCategoryId.Value, out var parentCategory))
                {
                    SetParentCategory(category, parentCategory);
                }
            }

            // Find all child categories
            var childCategories = categoryDtos
                .Where(dto => dto.ParentCategoryId.HasValue && dto.ParentCategoryId.Value == category.Id)
                .Select(dto => dto.Id)
                .ToList();

            // Add child categories to the SubCategories collection
            foreach (var childId in childCategories)
            {
                if (categoryLookup.TryGetValue(childId, out var childCategory))
                {
                    var field = typeof(Category).GetField("_subCategories",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (field != null && field.GetValue(category) is List<Category> subCategories)
                    {
                        subCategories.Add(childCategory);
                    }
                }
            }
        }
    }

    #endregion
}