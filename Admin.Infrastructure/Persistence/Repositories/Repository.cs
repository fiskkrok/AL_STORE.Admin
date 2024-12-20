using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;

using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Repositories;
public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : AuditableEntity
{
    protected readonly AdminDbContext Context;
    private readonly ILogger<Repository<TEntity>> _logger;

    protected Repository(
        AdminDbContext context,
        ILogger<Repository<TEntity>> logger)
    {
        Context = context;
        _logger = logger;
    }

    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.Set<TEntity>()
                .FindAsync(new object[] { id }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                id);
            throw;
        }
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
            _logger.LogInformation(
                "Added {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                entity.Id);
            throw;
        }
    }

    public virtual Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Set<TEntity>().Update(entity);
            _logger.LogInformation(
                "Updated {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                entity.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                entity.Id);
            throw;
        }
    }

    public virtual Task RemoveAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Context.Set<TEntity>().Remove(entity);
            _logger.LogInformation(
                "Removed {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                entity.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing {EntityType} with ID {Id}",
                typeof(TEntity).Name,
                entity.Id);
            throw;
        }
    }
}
