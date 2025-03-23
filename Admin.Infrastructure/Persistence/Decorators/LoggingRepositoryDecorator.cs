using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;
public class LoggingRepositoryDecorator<TEntity> : IRepository<TEntity> where TEntity : AuditableEntity
{
    private readonly IRepository<TEntity> _inner;
    private readonly ILogger _logger;

    public LoggingRepositoryDecorator(IRepository<TEntity> inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
        var result = await _inner.GetByIdAsync(id, cancellationToken);
        _logger.LogDebug("Retrieved {EntityType} with ID {Id}: {EntityFound}",
            typeof(TEntity).Name, id, result != null);
        return result;
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
        await _inner.AddAsync(entity, cancellationToken);
        _logger.LogInformation("Added {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
        await _inner.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Removing {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
        await _inner.RemoveAsync(entity, cancellationToken);
        _logger.LogInformation("Removed {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
    }
}
