using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence.Seeder;
using Admin.Infrastructure.Services;
using Admin.WebAPI.Services;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Core services configuration
/// </summary>
public static class CoreServicesConfiguration
{
    /// <summary>
    /// Adds essential services required by the application
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Add logging
        services.AddLogging();

        // Add HttpContext
        services.AddHttpContextAccessor();

        // Add system clock
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Register CurrentUser service
        services.AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }

    /// <summary>
    /// Adds request logging middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path);
            await next.Invoke();
            logger.LogInformation("Finished handling request.");
        });

        return app;
    }

    /// <summary>
    /// Seeds the database when the application starts
    /// </summary>
    public static async Task<IApplicationBuilder> SeedDatabase(this IApplicationBuilder app)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;
            var seeder = services.GetRequiredService<IDbSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }

        return app;
    }
}