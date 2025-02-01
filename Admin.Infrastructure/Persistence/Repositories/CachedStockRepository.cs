using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Persistence.Repositories;
public class CachedStockRepository : IStockRepository
{
    private readonly IStockRepository _innerRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedStockRepository> _logger;
    private readonly CacheSettings _settings;

    private const string StockItemKeyPrefix = "stock:item:";
    private const string StockProductKeyPrefix = "stock:product:";
    private const string StockLowKeyPrefix = "stock:low";
    private const string StockOutKeyPrefix = "stock:out";
    private const string ReservationKeyPrefix = "stock:reservation:";

    public CachedStockRepository(
        IStockRepository innerRepository,
        ICacheService cache,
        IOptions<CacheSettings> settings,
        ILogger<CachedStockRepository> logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<StockItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{StockItemKeyPrefix}{id}";

            var cached = await _cache.GetAsync<StockItem>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for stock item {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for stock item {Id}", id);
            var stockItem = await _innerRepository.GetByIdAsync(id, cancellationToken);

            if (stockItem != null)
            {
                await _cache.SetAsync(
                    cacheKey,
                    stockItem,
                    _settings.DefaultExpiration,
                    cancellationToken);
            }

            return stockItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock item {Id} from cache", id);
            return await _innerRepository.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{StockProductKeyPrefix}{productId}";

            var cached = await _cache.GetAsync<StockItem>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product stock {ProductId}", productId);
                return cached;
            }

            _logger.LogDebug("Cache miss for product stock {ProductId}", productId);
            var stockItem = await _innerRepository.GetByProductIdAsync(productId, cancellationToken);

            if (stockItem != null)
            {
                await _cache.SetAsync(
                    cacheKey,
                    stockItem,
                    _settings.DefaultExpiration,
                    cancellationToken);
            }

            return stockItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock for product {ProductId} from cache", productId);
            return await _innerRepository.GetByProductIdAsync(productId, cancellationToken);
        }
    }

    public async Task<List<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Short cache duration for low stock items as they change frequently
            var cacheKey = $"{StockLowKeyPrefix}:list";
            var shortDuration = TimeSpan.FromMinutes(5);

            var cached = await _cache.GetAsync<List<StockItem>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for low stock items");
                return cached;
            }

            _logger.LogDebug("Cache miss for low stock items");
            var items = await _innerRepository.GetLowStockItemsAsync(cancellationToken);

            await _cache.SetAsync(
                cacheKey,
                items,
                shortDuration,
                cancellationToken);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items from cache");
            return await _innerRepository.GetLowStockItemsAsync(cancellationToken);
        }
    }

    public async Task<List<StockItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Short cache duration for out of stock items
            var cacheKey = $"{StockOutKeyPrefix}:list";
            var shortDuration = TimeSpan.FromMinutes(5);

            var cached = await _cache.GetAsync<List<StockItem>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for out of stock items");
                return cached;
            }

            _logger.LogDebug("Cache miss for out of stock items");
            var items = await _innerRepository.GetOutOfStockItemsAsync(cancellationToken);

            await _cache.SetAsync(
                cacheKey,
                items,
                shortDuration,
                cancellationToken);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving out of stock items from cache");
            return await _innerRepository.GetOutOfStockItemsAsync(cancellationToken);
        }
    }

    public async Task<bool> HasSufficientStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        // Don't cache this as it needs to be real-time
        return await _innerRepository.HasSufficientStockAsync(productId, quantity, cancellationToken);
    }

    public async Task<Dictionary<Guid, bool>> CheckStockAvailabilityAsync(
        Dictionary<Guid, int> productQuantities,
        CancellationToken cancellationToken = default)
    {
        // Don't cache this as it needs to be real-time
        return await _innerRepository.CheckStockAvailabilityAsync(productQuantities, cancellationToken);
    }

    public async Task ReserveStockAsync(Guid productId, Guid? variantId, int quantity, Guid id, CancellationToken ct)
    {
        await _innerRepository.ReserveStockAsync(productId, variantId, quantity, id, ct);
    }

    public async Task<StockReservation?> GetReservationAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{ReservationKeyPrefix}{orderId}";

            var cached = await _cache.GetAsync<StockReservation>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for reservation {OrderId}", orderId);
                return cached;
            }

            _logger.LogDebug("Cache miss for reservation {OrderId}", orderId);
            var reservation = await _innerRepository.GetReservationAsync(orderId, cancellationToken);

            if (reservation != null)
            {
                // Cache reservations with a short duration
                await _cache.SetAsync(
                    cacheKey,
                    reservation,
                    TimeSpan.FromMinutes(5),
                    cancellationToken);
            }

            return reservation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation {OrderId} from cache", orderId);
            return await _innerRepository.GetReservationAsync(orderId, cancellationToken);
        }
    }

    public Task<List<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        // Don't cache expired reservations as they need to be real-time
        return _innerRepository.GetExpiredReservationsAsync(cancellationToken);
    }

    public async Task AddAsync(StockItem entity, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(entity, cancellationToken);
        await InvalidateStockCacheAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(StockItem entity, CancellationToken cancellationToken = default)
    {
        await _innerRepository.UpdateAsync(entity, cancellationToken);
        await InvalidateStockCacheAsync(entity, cancellationToken);
    }

    public async Task RemoveAsync(StockItem entity, CancellationToken cancellationToken = default)
    {
        await _innerRepository.RemoveAsync(entity, cancellationToken);
        await InvalidateStockCacheAsync(entity, cancellationToken);
    }

    private async Task InvalidateStockCacheAsync(StockItem stockItem, CancellationToken cancellationToken)
    {
        try
        {
            var keysToInvalidate = new List<string>
            {
                $"{StockItemKeyPrefix}{stockItem.Id}",
                $"{StockProductKeyPrefix}{stockItem.ProductId}",
                $"{StockLowKeyPrefix}:list",
                $"{StockOutKeyPrefix}:list"
            };

            // Batch the invalidation tasks
            var tasks = keysToInvalidate.Select(key =>
                _cache.RemoveAsync(key, cancellationToken));

            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "Successfully invalidated {Count} cache entries for stock item {StockItemId}",
                keysToInvalidate.Count,
                stockItem.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for stock item {StockItemId}. Cache may be in inconsistent state.",
                stockItem.Id);
            throw;
        }
    }
}
