using Admin.Application;
using Admin.Infrastructure.Extensions;
using Admin.Infrastructure.Persistence;
using Admin.WebAPI.Configurations;

namespace Admin.WebAPI.Extensions;

/// <summary>
/// Main entry point for application configuration
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Configures all application services
    /// </summary>
    public static void ConfigureServices(this IHostApplicationBuilder builder)
    {
        // Application 
        builder.Services.AddApplication();
        // Core services
        builder.Services.AddCoreServices();

        // Infrastructure services
        builder.Services.AddApplicationServices();
        builder.AddDatabaseServices(builder.Configuration);
        builder.Services.AddMessagingServices(builder.Configuration);
        builder.Services.AddStorageServices(builder.Configuration);
        builder.AddCachingServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        // API services
        builder.Services.AddApiServices(builder.Configuration);

        // Security
        builder.Services.AddAuthenticationServices(builder.Configuration);
        builder.Services.AddAuthorizationServices();

        // Monitoring
        builder.Services.AddMonitoringServices(builder.Configuration);

    }

    /// <summary>
    /// Configures application request pipeline
    /// </summary>
    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app)
    {
        // Development-specific middleware
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.RouteTemplate = "api-docs/{documentName}/swagger.json");
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api-docs/v1/swagger.json", "Admin API V1");
                options.RoutePrefix = "api-docs";
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                options.DefaultModelsExpandDepth(-1);
                options.InjectStylesheet("/swagger-ui/custom.css");
                options.EnableFilter();
                options.DisplayRequestDuration();
            });
        }

        // Global middleware
        app.UseErrorHandling();
        app.UseRequestLogging();
        app.UseCors();
        app.UseHttpsRedirection();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // API Endpoints
        app.UseApiEndpoints();
        app.UseHealthEndpoints();

        // SignalR and Health checks
        app.MapSignalRHubs();
        app.UseHealthChecks();
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
                // Seed database
                await app.SeedDatabase();
                context.Database.EnsureCreated();
            }
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        return app;
    }
}