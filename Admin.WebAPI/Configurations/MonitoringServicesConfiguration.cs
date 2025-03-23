using Admin.Infrastructure.Persistence;
using Admin.WebAPI.Services;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Monitoring services configuration
/// </summary>
public static class MonitoringServicesConfiguration
{
    /// <summary>
    /// Adds health checks and other monitoring services
    /// </summary>
    public static IServiceCollection AddMonitoringServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>(
                "redis_health_check",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "cache", "redis" })
            .AddDbContextCheck<AdminDbContext>(
                "database_health_check",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "sql" });

        // Add RabbitMQ health check if RabbitMQ is configured
        var rabbitHost = configuration["RabbitMQ:Host"];
        var rabbitUsername = configuration["RabbitMQ:Username"];
        var rabbitPassword = configuration["RabbitMQ:Password"];
        var rabbitVirtualHost = configuration["RabbitMQ:VirtualHost"];

        if (!string.IsNullOrEmpty(rabbitHost) && !string.IsNullOrEmpty(rabbitUsername) && !string.IsNullOrEmpty(rabbitPassword))
        {
            var rabbitConnectionString = $"amqp://{rabbitUsername}:{rabbitPassword}@{rabbitHost}/{rabbitVirtualHost ?? "/"}";

            services.AddHealthChecks()
                .AddRabbitMQ(
                    rabbitConnectionString: rabbitConnectionString,
                    name: "rabbitmq_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "messaging", "rabbitmq" });
        }

        return services;
    }

    /// <summary>
    /// Configures health check endpoints
    /// </summary>
    public static IEndpointRouteBuilder UseHealthChecks(this IEndpointRouteBuilder app)
    {
        // Main health check endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Status = report.Status.ToString(),
                    Duration = report.TotalDuration,
                    Info = report.Entries.Select(e => new
                    {
                        Key = e.Key,
                        Status = e.Value.Status.ToString(),
                        Duration = e.Value.Duration,
                        Description = e.Value.Description,
                        Data = e.Value.Data
                    })
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        });

        // Component-specific health checks
        app.MapHealthChecks("/health/cache", new HealthCheckOptions
        {
            Predicate = (check) => check.Tags.Contains("cache")
        });

        app.MapHealthChecks("/health/database", new HealthCheckOptions
        {
            Predicate = (check) => check.Tags.Contains("database")
        });

        app.MapHealthChecks("/health/messaging", new HealthCheckOptions
        {
            Predicate = (check) => check.Tags.Contains("messaging")
        });

        return app;
    }
}