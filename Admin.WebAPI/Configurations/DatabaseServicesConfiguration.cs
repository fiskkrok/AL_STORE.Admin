using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Extensions;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Persistence.Seeder;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public static void AddDatabaseServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.AddSqlServerDbContext<AdminDbContext>("AdminConnection", null, 
            sqlOptions =>
        {
            var options = new SqlServerDbContextOptionsBuilder(sqlOptions);
            options.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName);
            options.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            options.CommandTimeout(30);
            // Use connection pooling effectively
            options.MinBatchSize(5);
            options.MaxBatchSize(200);
            // Add query splitting to prevent cartesian explosions on complex joins
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        //builder.Services.AddDbContext<AdminDbContext>(options =>
        //    options.UseSqlServer(
        //        configuration.GetConnectionString("StoreConnection"),
        //        sqlOptions =>
        //        {
        //            sqlOptions.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName);
        //            sqlOptions.EnableRetryOnFailure(
        //                maxRetryCount: 5,
        //                maxRetryDelay: TimeSpan.FromSeconds(30),
        //                errorNumbersToAdd: null);
        //            sqlOptions.CommandTimeout(30);
        //            // Use connection pooling effectively
        //            sqlOptions.MinBatchSize(5);
        //            sqlOptions.MaxBatchSize(200);
        //            // Add query splitting to prevent cartesian explosions on complex joins
        //            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        //        }));
        // Add unit of work
        builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdminDbContext>());
        // In Program.cs or Startup.cs

        builder.Services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AdminDbContext>());
        // Add repositories
        RegisterRepositories(builder.Services);

        // Add database seeders
        RegisterSeeders(builder.Services);

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