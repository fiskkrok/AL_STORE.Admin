using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;
public class CachingRepositoryDecorator<TEntity> : IRepository<TEntity> where TEntity : AuditableEntity
{
    private readonly IRepository<TEntity> _inner;
    private readonly ICacheService _cache;
    private readonly ILogger _logger;
    private readonly string _cacheKeyPrefix;
    private readonly TimeSpan _cacheExpiration;

    public CachingRepositoryDecorator(
        IRepository<TEntity> inner,
        ICacheService cache,
        ILogger logger,
        string cacheKeyPrefix,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheKeyPrefix = cacheKeyPrefix;
        _cacheExpiration = cacheExpiration;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{_cacheKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<TEntity>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return cached;
            }

            _logger.LogDebug("Cache miss for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            var entity = await _inner.GetByIdAsync(id, cancellationToken);

            if (entity != null)
            {
                await _cache.SetAsync(cacheKey, entity, _cacheExpiration, cancellationToken);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for {EntityType} with ID {Id}, falling back to repository",
                typeof(TEntity).Name, id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        // No caching for add operations
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);

        // Invalidate cache
        try
        {
            var cacheKey = $"{_cacheKeyPrefix}:{entity.Id}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Invalidated cache for {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for {EntityType} with ID {Id}",
                typeof(TEntity).Name, entity.Id);
            // Don't rethrow, as the primary update was successful
        }
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);

        // Invalidate cache
        try
        {
            var cacheKey = $"{_cacheKeyPrefix}:{entity.Id}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Invalidated cache for {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for {EntityType} with ID {Id}",
                typeof(TEntity).Name, entity.Id);
            // Don't rethrow, as the primary remove was successful
        }
    }
}