using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;

namespace Admin.Infrastructure.Persistence.Repositories;
public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : AuditableEntity
{
    protected readonly AdminDbContext Context;

    protected Repository(AdminDbContext context)
    {
        Context = context;
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual void Add(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    public virtual void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }
}
