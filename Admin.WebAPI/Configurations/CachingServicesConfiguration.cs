using Admin.Application.Common.Interfaces;
using Admin.WebAPI.Services;

using Microsoft.Extensions.Caching.Hybrid;

using StackExchange.Redis;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Caching services configuration
/// </summary>
public static class CachingServicesConfiguration
{
    /// <summary>
    /// Adds caching services, including Redis and hybrid cache
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Get Redis connection string
        var redisConnectionString = configuration.GetConnectionString("Redis") ??
                                    throw new InvalidOperationException("Redis connection string is not configured.");

        // Add Redis connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        // Add Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "Admin_";
        });

        // Add cache service
        services.AddScoped<ICacheService, RedisCacheService>();

        // Add hybrid cache (in-memory + Redis)
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            };
            options.MaximumPayloadBytes = 5 * 1024 * 1024; // 5MB
        });
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        // Add cache warming service
        services.AddHostedService<CacheWarmingService>();

        return services;
    }
}