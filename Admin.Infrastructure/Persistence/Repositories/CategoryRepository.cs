using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Repositories;
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly AdminDbContext _context;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(
        AdminDbContext context,
        ILogger<CategoryRepository> logger) : base(context, logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category with ID: {CategoryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return await query
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all categories");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching root categories");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == id && c.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking category existence: {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> HasChildrenAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Categories
                .AnyAsync(c => c.ParentCategoryId == id && c.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking category children: {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> HasProductsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == id && p.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking category products: {CategoryId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetBreadcrumbAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var breadcrumb = new List<Category>();
            var category = await GetByIdAsync(categoryId, cancellationToken);

            while (category != null)
            {
                breadcrumb.Insert(0, category);
                category = category.ParentCategory;
            }

            return breadcrumb;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category breadcrumb: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<bool> IsSlugUniqueAsync(
        string slug,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Categories
                .Where(c => c.Slug == slug && c.IsActive);

            if (excludeCategoryId.HasValue)
                query = query.Where(c => c.Id != excludeCategoryId.Value);

            return !await query.AnyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking slug uniqueness: {Slug}", slug);
            throw;
        }
    }

    public async Task ReorderCategoriesAsync(
        IEnumerable<(Guid CategoryId, int NewOrder)> newOrders,
        CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var (categoryId, newOrder) in newOrders)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive, cancellationToken);

                if (category != null)
                {
                    category.UpdateSortOrder(newOrder);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering categories");
            throw;
        }
    }

    public async Task<IEnumerable<Category>> SearchCategoriesAsync(
        string searchTerm,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .Where(c => c.IsActive &&
                           (c.Name.Contains(searchTerm) ||
                            c.Description.Contains(searchTerm) ||
                            c.Slug.Contains(searchTerm)))
                .OrderBy(c => c.Name);

            if (maxResults.HasValue)
                query = (IOrderedQueryable<Category>)query.Take(maxResults.Value);

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetCategoryPathAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var path = new List<Category>();
            var category = await GetByIdAsync(categoryId, cancellationToken);

            if (category == null)
                return path;

            path.Add(category);

            // Get all subcategories recursively
            await AddSubcategoriesRecursively(category, path, cancellationToken);

            return path;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category path: {CategoryId}", categoryId);
            throw;
        }
    }

    private async Task AddSubcategoriesRecursively(
        Category parent,
        List<Category> path,
        CancellationToken cancellationToken)
    {
        var subcategories = await _context.Categories
            .Where(c => c.ParentCategoryId == parent.Id && c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var subcategory in subcategories)
        {
            path.Add(subcategory);
            await AddSubcategoriesRecursively(subcategory, path, cancellationToken);
        }
    }
}
