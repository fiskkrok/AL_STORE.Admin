using System.Text.Json;

using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Seeder;

public class ProductDbSeeder : IProductSeeder
{
    private readonly AdminDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProductDbSeeder> _logger;
    private const int TOTAL_IMAGES = 29;
    private int _currentImageIndex = 1;

    public ProductDbSeeder(
        AdminDbContext context,
        IWebHostEnvironment env,
        ILogger<ProductDbSeeder> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

    private static string GetFullPath(string contentRootPath, string pictureFileName) =>
        Path.Combine(contentRootPath, "Pics", pictureFileName);

    private static string GetImageMimeType(string extension) => extension switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        ".webp" => "image/webp",
        _ => "application/octet-stream",
    };

    public async Task SeedAsync()
    {
        try
        {
            if (!_context.Products.Any())
            {
                var sourcePath = Path.Combine(_env.ContentRootPath, "Setup", "product.json");
                var sourceJson = await File.ReadAllTextAsync(sourcePath);
                var seedData = JsonSerializer.Deserialize<ProductSeedData>(sourceJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Get all categories with their subcategories
                var categories = await _context.Categories
                    .Include(c => c.SubCategories)
                    .ToListAsync();

                foreach (var productGroup in seedData.Products)
                {
                    // Find the main category and subcategory
                    var mainCategory = categories.FirstOrDefault(c => c.Name == productGroup.Genre);
                    var subCategory = categories.FirstOrDefault(c => c.Name == productGroup.SubGenre);

                    if (mainCategory != null)
                    {
                        foreach (var item in productGroup.Items)
                        {
                            var imageFileName = $"{_currentImageIndex}.webp";
                            var imagePath = GetFullPath(_env.ContentRootPath, imageFileName);

                            // Generate a SKU if one is not provided
                            var sku = string.IsNullOrEmpty(item.Sku)
                                ? $"{item.Brand}-{Guid.NewGuid().ToString().Substring(0, 8)}"
                                : item.Sku;

                            // Only add image if file exists
                            if (File.Exists(imagePath))
                            {
                                var product = new Product(
                                    name: item.Name,
                                    description: item.Description,
                                    price: item.Price,
                                    currency: "USD",
                                    sku: sku,  // Using the generated or provided SKU
                                    stock: item.Stock,
                                    categoryId: mainCategory.Id,
                                    subCategoryId: subCategory?.Id);

                                var fileInfo = new FileInfo(imagePath);
                                product.AddImage(
                                    url: imageFileName,
                                    fileName: imageFileName,
                                    size: fileInfo.Length
                                );

                                _context.Products.Add(product);

                                // Rotate image index
                                _currentImageIndex = (_currentImageIndex % TOTAL_IMAGES) + 1;
                            }
                            else
                            {
                                _logger.LogWarning($"Image file not found: {imagePath}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Category not found for {productGroup.Genre}");
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Products seeded successfully");
            }
            else
            {
                _logger.LogInformation("Products already exist - skipping seed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding products: {Message}", ex.Message);
            throw;
        }
    }
}

// Keep existing model classes
public class ProductSeedData
{
    public List<ProductGroup> Products { get; set; } = new();
}

public class ProductGroup
{
    public string Genre { get; set; } = string.Empty;
    public string SubGenre { get; set; } = string.Empty;
    public List<ProductItem> Items { get; set; } = new();
}

public class ProductItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Sku { get; set; } = string.Empty;
}