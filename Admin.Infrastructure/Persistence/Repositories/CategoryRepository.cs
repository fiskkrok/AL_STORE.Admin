using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Repositories;
public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AdminDbContext context) : base(context)
    {
    }

    public override async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Categories
            .Include(c => c.ParentCategory)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Categories
            .AnyAsync(c => c.Id == id && c.IsActive, cancellationToken);
    }
    public override async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        // Ensure parent category is properly tracked if it exists
        if (entity.ParentCategoryId.HasValue)
        {
            var parent = await DbContext.Categories.FindAsync(entity.ParentCategoryId.Value);
            if (parent != null)
            {
                DbContext.Entry(parent).State = EntityState.Unchanged;
            }
        }

        await DbContext.Categories.AddAsync(entity, cancellationToken);
    }
}
