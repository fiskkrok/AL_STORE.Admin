using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence.Decorators;
using Admin.Infrastructure.Persistence.Repositories;
using Admin.Infrastructure.Services.Caching.Mappings;
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
    /// Adds caching services, including Redis and hybrid cache with DTO-based caching
    /// </summary>
    public static void AddCachingServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        // Get Redis connection string
        //var redisConnectionString = configuration.GetConnectionString("Redis") ??
        //                            throw new InvalidOperationException("Redis connection string is not configured.");

        //// Add Redis connection
        //services.AddSingleton<IConnectionMultiplexer>(sp =>
        //    ConnectionMultiplexer.Connect(redisConnectionString));

        //// Add Redis distributed cache
        //services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = redisConnectionString;
        //    options.InstanceName = "Admin_";
        //    if (options.ConfigurationOptions != null)
        //    {
        //        options.ConfigurationOptions.ConnectTimeout = 10000; // 10 seconds
        //        options.ConfigurationOptions.SyncTimeout = 10000;
        //    }
        //});
        builder.AddRedisClient("redis");

        // Add cache service
        builder.Services.AddScoped<ICacheService, RedisCacheService>();

        // Add hybrid cache (in-memory + Redis)
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            };
            options.MaximumPayloadBytes = 5 * 1024 * 1024; // 5MB
        });
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        // Register cache mapping profiles for DTO-based caching
        builder.Services.AddAutoMapper(typeof(CacheMappingProfile).Assembly);

        // Configure repository decorators with DTO-based caching
        ConfigureRepositoryDecorators(builder.Services, configuration);

        // Add cache warming service
        builder.Services.AddHostedService<CacheWarmingService>();

        // Add Redis health check
        builder.Services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "cache", "redis" });

    }

    private static void ConfigureRepositoryDecorators(IServiceCollection services, IConfiguration configuration)
    {
        // Configure cache expiration from settings
        var cacheExpirationMinutes = configuration.GetValue("Redis:DefaultExpirationMinutes", 30);
        var cacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);

        // For Product repository
        services.Decorate<IProductRepository>((inner, provider) =>
            new CachingProductRepositoryDecorator(
                inner,
                provider.GetRequiredService<ICacheService>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<ILogger<CachingProductRepositoryDecorator>>(),
                cacheExpiration));

        // For Category repository
        services.Decorate<ICategoryRepository>((inner, provider) =>
            new CachingCategoryRepositoryDecorator(
                inner,
                provider.GetRequiredService<ICacheService>(),
                provider.GetRequiredService<AutoMapper.IMapper>(),
                provider.GetRequiredService<ILogger<CachingCategoryRepositoryDecorator>>(),
                cacheExpiration));
    }
}