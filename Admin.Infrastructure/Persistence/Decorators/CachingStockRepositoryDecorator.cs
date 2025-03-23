using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;

public class CachingStockRepositoryDecorator : IStockRepository
{
    private readonly IStockRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger _logger;
    private readonly TimeSpan _cacheExpiration;
    private readonly TimeSpan _shortExpiration;

    private const string StockItemKeyPrefix = "stock:item";
    private const string StockProductKeyPrefix = "stock:product";
    private const string StockLowKeyPrefix = "stock:low";
    private const string StockOutKeyPrefix = "stock:out";
    private const string ReservationKeyPrefix = "stock:reservation";

    public CachingStockRepositoryDecorator(
        IStockRepository inner,
        ICacheService cache,
        ILogger logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
        _shortExpiration = TimeSpan.FromMinutes(5); // Short expiration for frequently changing data
    }

    public async Task<StockItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StockItemKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<StockItem>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for stock item with ID {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for stock item with ID {Id}", id);
            var stockItem = await _inner.GetByIdAsync(id, cancellationToken);

            if (stockItem != null)
            {
                await _cache.SetAsync(cacheKey, stockItem, _cacheExpiration, cancellationToken);
            }

            return stockItem;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for stock item with ID {Id}, falling back to repository", id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StockProductKeyPrefix}:{productId}";

        try
        {
            var cached = await _cache.GetAsync<StockItem>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for product stock with ID {ProductId}", productId);
                return cached;
            }

            _logger.LogDebug("Cache miss for product stock with ID {ProductId}", productId);
            var stockItem = await _inner.GetByProductIdAsync(productId, cancellationToken);

            if (stockItem != null)
            {
                await _cache.SetAsync(cacheKey, stockItem, _cacheExpiration, cancellationToken);
            }

            return stockItem;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for product stock with ID {ProductId}, falling back to repository", productId);
            return await _inner.GetByProductIdAsync(productId, cancellationToken);
        }
    }

    public async Task<List<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StockLowKeyPrefix}:list";

        try
        {
            var cached = await _cache.GetAsync<List<StockItem>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for low stock items");
                return cached;
            }

            _logger.LogDebug("Cache miss for low stock items");
            var items = await _inner.GetLowStockItemsAsync(cancellationToken);

            // Cache with short expiration as this data changes frequently
            await _cache.SetAsync(cacheKey, items, _shortExpiration, cancellationToken);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for low stock items, falling back to repository");
            return await _inner.GetLowStockItemsAsync(cancellationToken);
        }
    }

    public async Task<List<StockItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{StockOutKeyPrefix}:list";

        try
        {
            var cached = await _cache.GetAsync<List<StockItem>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for out of stock items");
                return cached;
            }

            _logger.LogDebug("Cache miss for out of stock items");
            var items = await _inner.GetOutOfStockItemsAsync(cancellationToken);

            // Cache with short expiration as this data changes frequently
            await _cache.SetAsync(cacheKey, items, _shortExpiration, cancellationToken);

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for out of stock items, falling back to repository");
            return await _inner.GetOutOfStockItemsAsync(cancellationToken);
        }
    }

    public async Task<StockReservation?> GetReservationAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ReservationKeyPrefix}:{orderId}";

        try
        {
            var cached = await _cache.GetAsync<StockReservation>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for reservation with order ID {OrderId}", orderId);
                return cached;
            }

            _logger.LogDebug("Cache miss for reservation with order ID {OrderId}", orderId);
            var reservation = await _inner.GetReservationAsync(orderId, cancellationToken);

            if (reservation != null)
            {
                // Short cache time for reservations
                await _cache.SetAsync(cacheKey, reservation, _shortExpiration, cancellationToken);
            }

            return reservation;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for reservation with order ID {OrderId}, falling back to repository", orderId);
            return await _inner.GetReservationAsync(orderId, cancellationToken);
        }
    }

    // Don't cache real-time operations - pass through to inner
    public Task<List<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        return _inner.GetExpiredReservationsAsync(cancellationToken);
    }

    public Task<bool> HasSufficientStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        return _inner.HasSufficientStockAsync(productId, quantity, cancellationToken);
    }

    public Task<Dictionary<Guid, bool>> CheckStockAvailabilityAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default)
    {
        return _inner.CheckStockAvailabilityAsync(productQuantities, cancellationToken);
    }

    public async Task ReserveStockAsync(Guid productId, Guid? variantId, int quantity, Guid id, CancellationToken ct)
    {
        await _inner.ReserveStockAsync(productId, variantId, quantity, id, ct);
        await InvalidateStockCacheAsync(productId, ct);
    }

    public async Task AddAsync(StockItem entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        await InvalidateStockCacheAsync(entity.ProductId, cancellationToken);
    }

    public async Task UpdateAsync(StockItem entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        await InvalidateStockCacheAsync(entity.ProductId, cancellationToken);
    }

    public async Task RemoveAsync(StockItem entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        await InvalidateStockCacheAsync(entity.ProductId, cancellationToken);
    }

    private async Task InvalidateStockCacheAsync(Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            var keysToInvalidate = new List<string>
            {
                $"{StockProductKeyPrefix}:{productId}",
                $"{StockLowKeyPrefix}:list",
                $"{StockOutKeyPrefix}:list"
            };

            foreach (var key in keysToInvalidate)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Invalidated cache key {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for stock with product ID {ProductId}", productId);
        }
    }
}