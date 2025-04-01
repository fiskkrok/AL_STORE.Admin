using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;

using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Repositories;

public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : AuditableEntity
{
    protected readonly AdminDbContext DbContext;
    protected readonly DbSet<TEntity> EntitySet;

    public BaseRepository(AdminDbContext context)
    {
        DbContext = context;
        EntitySet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await EntitySet.FindAsync([id], cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await EntitySet.AddAsync(entity, cancellationToken);
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        EntitySet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        EntitySet.Remove(entity);
        return Task.CompletedTask;
    }
}