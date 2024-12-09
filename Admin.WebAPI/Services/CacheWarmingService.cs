using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Queries;

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
            var productRepository = scope.ServiceProvider
                .GetRequiredService<IProductRepository>();
            var cache = scope.ServiceProvider
                .GetRequiredService<ICacheService>();

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
                await cache.SetAsync(
                    $"product:{product.Id}",
                    product,
                    TimeSpan.FromMinutes(60),
                    cancellationToken);

                foreach (var variant in product.Variants)
                {
                    await cache.SetAsync(
                        $"variant:{variant.Id}",
                        variant,
                        TimeSpan.FromMinutes(60),
                        cancellationToken);
                }
            }

            _logger.LogInformation("Cache warming completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warming");
            // Don't throw - we don't want to prevent app startup
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
