using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Extensions;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Persistence.Seeder;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        services.AddDbContext<AdminDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                    // Use connection pooling effectively
                    sqlOptions.MinBatchSize(5);
                    sqlOptions.MaxBatchSize(200);
                    // Add query splitting to prevent cartesian explosions on complex joins
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));
        // Add unit of work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdminDbContext>());
        // In Program.cs or Startup.cs
        
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AdminDbContext>());
        // Add repositories
        RegisterRepositories(services);

        // Add database seeders
        RegisterSeeders(services);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddRepositories();
    }

    private static void RegisterSeeders(IServiceCollection services)
    {
        services.AddScoped<ICategorySeeder, CategoryDbSeeder>();
        services.AddScoped<IProductSeeder, ProductDbSeeder>();
        services.AddScoped<IProductTypeSeeder, ProductTypeDbSeeder>();
        services.AddScoped<IDbSeeder, MainDbSeeder>();
    }
}