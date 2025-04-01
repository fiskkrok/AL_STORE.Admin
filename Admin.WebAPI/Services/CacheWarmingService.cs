using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;
using Admin.Infrastructure.Services.Caching.DTOs;

using AutoMapper;

namespace Admin.WebAPI.Services;

public class CacheWarmingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmingService> _logger;
    private readonly IConfiguration _configuration;

    public CacheWarmingService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<CacheWarmingService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting cache warming...");

            using var scope = _serviceProvider.CreateScope();
            var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            // Warm up frequently accessed products
            var featuredProducts = await productRepository.GetProductsAsync(
                new ProductFilterRequest
                {
                    Visibility = "Featured",
                    PageSize = 20
                },
                cancellationToken);

            foreach (var product in featuredProducts.Products)
            {
                // Convert to cache DTO
                var productDto = mapper.Map<ProductCacheDto>(product);

                // Cache the DTO
                await cache.SetAsync(
                    $"product:dto:{product.Id}",
                    productDto,
                    TimeSpan.FromMinutes(60),
                    cancellationToken);

                // Cache variants
                foreach (var variant in product.Variants)
                {
                    var variantDto = mapper.Map<ProductVariantCacheDto>(variant);
                    await cache.SetAsync(
                        $"variant:dto:{variant.Id}",
                        variantDto,
                        TimeSpan.FromMinutes(60),
                        cancellationToken);
                }
            }

            // Warm up categories
            var categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
            var categories = await categoryRepository.GetAllAsync(false, cancellationToken);

            var categoryDtos = mapper.Map<List<CategoryCacheDto>>(categories);
            await cache.SetAsync(
                "categories:list:dto:False",
                categoryDtos,
                TimeSpan.FromMinutes(60),
                cancellationToken);

            _logger.LogInformation("Cache warming completed successfully. Cached {ProductCount} products and {CategoryCount} categories",
                featuredProducts.Products.Count(), categoryDtos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warming");
            // Don't throw - we don't want to prevent app startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}