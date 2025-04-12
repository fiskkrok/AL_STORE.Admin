using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Seeder;

public class MainDbSeeder : IDbSeeder
{
    private readonly AdminDbContext _context;
    private readonly ICategorySeeder _categorySeeder;
    private readonly IProductSeeder _productSeeder;
    private readonly IProductTypeSeeder _productTypeSeeder;
    private readonly ILogger<MainDbSeeder> _logger;

    public MainDbSeeder(
        AdminDbContext context,
        ICategorySeeder categorySeeder,
        IProductSeeder productSeeder,
        ILogger<MainDbSeeder> logger,
        IProductTypeSeeder productTypeSeeder)
    {
        _context = context;
        _categorySeeder = categorySeeder;
        _productSeeder = productSeeder;
        _logger = logger;
        _productTypeSeeder = productTypeSeeder;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Checking if seeding is required...");

            var hasProducts = await _context.Products.AnyAsync();
            var hasCategories = await _context.Categories.AnyAsync();
            var hasProductTypes = await _context.ProductTypes.AnyAsync();

            if (hasProducts && hasCategories && hasProductTypes)
            {
                _logger.LogInformation("Database already contains data, skipping seeding");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            if (!hasCategories)
            {
                _logger.LogInformation("Seeding categories...");
                await _categorySeeder.SeedAsync();
            }

            if (!hasProducts)
            {
                _logger.LogInformation("Seeding products...");
                await _productSeeder.SeedAsync();
            }

            if (!hasProductTypes)
            {
                _logger.LogInformation("Seeding product types...");
                await _productTypeSeeder.SeedAsync();
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
