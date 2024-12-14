using System.Text.Json;

using Admin.Domain.Entities;

using Microsoft.AspNetCore.Hosting;
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
                var sourcePath = Path.Combine(_env.ContentRootPath, "Setup", "categories.json");
                var sourceJson = await File.ReadAllTextAsync(sourcePath);
                var seedData = JsonSerializer.Deserialize<CategorySeedData>(sourceJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var categoryData in seedData.Categories)
                {
                    // Create main category
                    var mainCategory = new Category(categoryData.Name, categoryData.Description);
                    _context.Categories.Add(mainCategory);

                    if (categoryData.SubCategories != null)
                    {
                        foreach (var subData in categoryData.SubCategories)
                        {
                            var subCategory = new Category(subData.Name, subData.Description)
                            {
                                ParentCategoryId = mainCategory.Id
                            };
                            _context.Categories.Add(subCategory);

                            // Handle nested subcategories if any
                            if (subData.SubCategories != null)
                            {
                                foreach (var nestedSub in subData.SubCategories)
                                {
                                    var nestedCategory = new Category(nestedSub.Name, nestedSub.Description)
                                    {
                                        ParentCategoryId = subCategory.Id
                                    };
                                    _context.Categories.Add(nestedCategory);
                                }
                            }
                        }
                    }
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
            _logger.LogError(ex, "An error occurred while seeding categories: {Message}", ex.Message);
            throw;
        }
    }
}

// Models for deserializing the JSON
public class CategorySeedData
{
    public List<CategoryData> Categories { get; set; } = new();
}

public class CategoryData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CategoryData>? SubCategories { get; set; }
}