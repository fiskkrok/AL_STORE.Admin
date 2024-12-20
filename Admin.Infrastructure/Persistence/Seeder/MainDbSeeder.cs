using Admin.Infrastructure.Persistence.Seeder;
using Admin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class MainDbSeeder : IDbSeeder
{
    private readonly AdminDbContext _context;
    private readonly ICategorySeeder _categorySeeder;
    private readonly IProductSeeder _productSeeder;
    private readonly ILogger<MainDbSeeder> _logger;

    public MainDbSeeder(
        AdminDbContext context,
        ICategorySeeder categorySeeder,
        IProductSeeder productSeeder,
        ILogger<MainDbSeeder> logger)
    {
        _context = context;
        _categorySeeder = categorySeeder;
        _productSeeder = productSeeder;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Checking if seeding is required...");

            var hasData = await _context.Products.AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("Database already contains data, skipping seeding");
                return;
            }

            _logger.LogInformation("Starting database seeding...");
            await _categorySeeder.SeedAsync();
            await _productSeeder.SeedAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}