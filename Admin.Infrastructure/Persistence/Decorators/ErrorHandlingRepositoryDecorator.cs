using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Admin.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;
public class ErrorHandlingRepositoryDecorator<TEntity> : IRepository<TEntity> where TEntity : AuditableEntity
{
    private readonly IRepository<TEntity> _inner;
    private readonly ILogger _logger;

    public ErrorHandlingRepositoryDecorator(IRepository<TEntity> inner, ILogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            throw new RepositoryException($"Error retrieving {typeof(TEntity).Name}", ex);
        }
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _inner.AddAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
            throw new RepositoryException($"Error adding {typeof(TEntity).Name}", ex);
        }
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _inner.UpdateAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
            throw new RepositoryException($"Error updating {typeof(TEntity).Name}", ex);
        }
    }

    public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _inner.RemoveAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing {EntityType} with ID {Id}", typeof(TEntity).Name, entity.Id);
            throw new RepositoryException($"Error removing {typeof(TEntity).Name}", ex);
        }
    }
}
