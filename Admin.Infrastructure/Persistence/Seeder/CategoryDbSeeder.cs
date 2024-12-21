using System.Text.Json;

using Admin.Domain.Entities;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Seeder;

public class CategoryDbSeeder : ICategorySeeder
{
    private readonly AdminDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CategoryDbSeeder> _logger;

    public CategoryDbSeeder(
        AdminDbContext context,
        IWebHostEnvironment env,
        ILogger<CategoryDbSeeder> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (!_context.Categories.Any())
            {
                _logger.LogInformation("Starting category seeding...");

                var sourcePath = Path.Combine(_env.ContentRootPath, "Setup", "categories.json");
                var sourceJson = await File.ReadAllTextAsync(sourcePath);
                var seedData = JsonSerializer.Deserialize<CategorySeedData>(sourceJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                if (seedData?.Categories == null)
                {
                    _logger.LogWarning("No categories found in seed data");
                    return;
                }

                // Create root categories first
                foreach (var categoryData in seedData.Categories)
                {
                    await CreateCategoryHierarchy(categoryData, null);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Categories seeded successfully");
            }
            else
            {
                _logger.LogInformation("Categories already exist - skipping seed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding categories");
            throw;
        }
    }

    private async Task CreateCategoryHierarchy(CategoryData data, Guid? parentId)
    {
        try
        {
            // Create base category with required fields only
            var category = new Category(
                name: data.Name,
                description: data.Description,
                imageUrl: data.ImageUrl,
                parent: parentId.HasValue ? await _context.Categories.FindAsync(parentId) : null);

            // Set the ID directly in the database context
            _context.Entry(category).Property("Id").CurrentValue = Guid.Parse(data.Id);

            // Update additional properties
            category.Update(
                data.Name,
                data.Description,
                data.MetaTitle,
                data.MetaDescription);

            category.UpdateSortOrder(data.SortOrder);

            // Set audit fields via context rather than reflection
            _context.Entry(category).Property("CreatedAt").CurrentValue = data.CreatedAt;
            _context.Entry(category).Property("CreatedBy").CurrentValue = data.CreatedBy;
            _context.Entry(category).Property("IsActive").CurrentValue = data.IsActive;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Process subcategories if any
            if (data.SubCategories != null)
            {
                foreach (var subCategoryData in data.SubCategories)
                {
                    await CreateCategoryHierarchy(subCategoryData, category.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category {CategoryName}", data.Name);
            throw;
        }
    }
}

public class CategorySeedData
{
    public List<CategoryData> Categories { get; set; } = new();
}

public class CategoryData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CategoryData>? SubCategories { get; set; }
}