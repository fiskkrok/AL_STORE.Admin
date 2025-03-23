using Admin.Application.Common.Interfaces;
using Admin.Application.Orders;
using Admin.Domain.Entities;
using Admin.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Decorators;
public class CachingOrderRepositoryDecorator : IOrderRepository
{
    private readonly IOrderRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger _logger;
    private readonly TimeSpan _cacheExpiration;

    private const string OrderKeyPrefix = "order:id";
    private const string OrderNumberKeyPrefix = "order:number";
    private const string CustomerOrdersKeyPrefix = "order:customer";
    private const string StatusOrdersKeyPrefix = "order:status";

    public CachingOrderRepositoryDecorator(
        IOrderRepository inner,
        ICacheService cache,
        ILogger logger,
        TimeSpan cacheExpiration)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
        _cacheExpiration = cacheExpiration;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{OrderKeyPrefix}:{id}";

        try
        {
            var cached = await _cache.GetAsync<Order>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for order with ID {Id}", id);
                return cached;
            }

            _logger.LogDebug("Cache miss for order with ID {Id}", id);
            var order = await _inner.GetByIdAsync(id, cancellationToken);

            if (order != null)
            {
                await _cache.SetAsync(cacheKey, order, _cacheExpiration, cancellationToken);
            }

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for order with ID {Id}, falling back to repository", id);
            return await _inner.GetByIdAsync(id, cancellationToken);
        }
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{OrderNumberKeyPrefix}:{orderNumber}";

        try
        {
            var cached = await _cache.GetAsync<Order>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for order number {OrderNumber}", orderNumber);
                return cached;
            }

            _logger.LogDebug("Cache miss for order number {OrderNumber}", orderNumber);
            var order = await _inner.GetByOrderNumberAsync(orderNumber, cancellationToken);

            if (order != null)
            {
                await _cache.SetAsync(cacheKey, order, _cacheExpiration, cancellationToken);
            }

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for order number {OrderNumber}, falling back to repository", orderNumber);
            return await _inner.GetByOrderNumberAsync(orderNumber, cancellationToken);
        }
    }

    // For list operations, we might want to be more selective about caching
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CustomerOrdersKeyPrefix}:{customerId}";

        try
        {
            var cached = await _cache.GetAsync<IEnumerable<Order>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for customer orders {CustomerId}", customerId);
                return cached;
            }

            _logger.LogDebug("Cache miss for customer orders {CustomerId}", customerId);
            var orders = await _inner.GetByCustomerIdAsync(customerId, cancellationToken);

            // Cache for a shorter period since order lists change frequently
            var shortExpiration = TimeSpan.FromMinutes(5);
            await _cache.SetAsync(cacheKey, orders, shortExpiration, cancellationToken);

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for customer orders {CustomerId}, falling back to repository", customerId);
            return await _inner.GetByCustomerIdAsync(customerId, cancellationToken);
        }
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        // For active statuses, don't cache as they change frequently
        if (status == OrderStatus.Processing || status == OrderStatus.Shipped)
        {
            return await _inner.GetByStatusAsync(status, cancellationToken);
        }

        var cacheKey = $"{StatusOrdersKeyPrefix}:{status}";

        try
        {
            var cached = await _cache.GetAsync<IEnumerable<Order>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for status orders {Status}", status);
                return cached;
            }

            _logger.LogDebug("Cache miss for status orders {Status}", status);
            var orders = await _inner.GetByStatusAsync(status, cancellationToken);

            // Cache completed/cancelled orders for longer
            await _cache.SetAsync(cacheKey, orders, _cacheExpiration, cancellationToken);

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing cache for status orders {Status}, falling back to repository", status);
            return await _inner.GetByStatusAsync(status, cancellationToken);
        }
    }

    // Don't cache filtered lists - too many permutations
    public Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersAsync(
        OrderFilterRequest filter, CancellationToken cancellationToken = default)
    {
        return _inner.GetOrdersAsync(filter, cancellationToken);
    }

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _inner.AddAsync(entity, cancellationToken);
        // When adding, we don't need to invalidate any caches since this is a new entity
    }

    public async Task UpdateAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(entity, cancellationToken);
        await InvalidateOrderCacheAsync(entity, cancellationToken);
    }

    public async Task RemoveAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await _inner.RemoveAsync(entity, cancellationToken);
        await InvalidateOrderCacheAsync(entity, cancellationToken);
    }

    private async Task InvalidateOrderCacheAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            var keysToInvalidate = new List<string>
            {
                $"{OrderKeyPrefix}:{order.Id}",
                $"{OrderNumberKeyPrefix}:{order.OrderNumber}",
                $"{CustomerOrdersKeyPrefix}:{order.CustomerId}",
                $"{StatusOrdersKeyPrefix}:{order.Status}"
            };

            foreach (var key in keysToInvalidate)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Invalidated cache key {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for order {OrderId}", order.Id);
        }
    }
}
