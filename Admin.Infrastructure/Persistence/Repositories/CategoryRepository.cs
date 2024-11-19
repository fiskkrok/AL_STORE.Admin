using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Repositories;
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AdminDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Categories
            .Include(c => c.SubCategories)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .AnyAsync(c => c.Id == id && c.IsActive, cancellationToken);
    }
}
