using Admin.Application.Common.Interfaces;
using Admin.Infrastructure;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Persistence.Repositories;
using Admin.Infrastructure.Persistence.Seeder;

using Microsoft.EntityFrameworkCore;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Database and repositories configuration
/// </summary>
public static class DatabaseServicesConfiguration
{
    /// <summary>
    /// Adds database context and repositories
    /// </summary>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {

        // Configure DbContext
        services.AddDbContext<AdminDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure();
                    b.CommandTimeout(30);
                }));

        // Add unit of work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdminDbContext>());

        // Add repositories
        RegisterRepositories(services);

        // Add database seeders
        RegisterSeeders(services);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        // Register base repositories
        //services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register specific repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Add repository decorators for caching
        services.Decorate<IProductRepository, CachedProductRepository>();
        services.Decorate<IStockRepository, CachedStockRepository>();
    }

    private static void RegisterSeeders(IServiceCollection services)
    {
        services.AddScoped<ICategorySeeder, CategoryDbSeeder>();
        services.AddScoped<IProductSeeder, ProductDbSeeder>();
        services.AddScoped<IDbSeeder, MainDbSeeder>();
    }
}