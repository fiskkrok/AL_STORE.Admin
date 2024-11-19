using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Seeder;
public class MainDbSeeder : IDbSeeder
{
    private readonly ICategorySeeder _categorySeeder;
    private readonly IProductSeeder _productSeeder;
    private readonly ILogger<MainDbSeeder> _logger;

    public MainDbSeeder(
        ICategorySeeder categorySeeder,
        IProductSeeder productSeeder,
        ILogger<MainDbSeeder> logger)
    {
        _categorySeeder = categorySeeder;
        _productSeeder = productSeeder;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Seed categories first
            await _categorySeeder.SeedAsync();

            // Then seed products
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