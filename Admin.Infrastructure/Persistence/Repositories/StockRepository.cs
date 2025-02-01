using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Repositories;
public class StockRepository : Repository<StockItem>, IStockRepository
{
    private readonly AdminDbContext _context;
    private readonly ILogger<StockRepository> _logger;

    public StockRepository(
        AdminDbContext context,
        ILogger<StockRepository> logger) : base(context, logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StockItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<StockItem>()
                .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
                .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock item for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<List<StockItem>> GetLowStockItemsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<StockItem>()
                .Where(x => x.IsLowStock && x.TrackInventory)
                .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
                .OrderBy(x => x.AvailableStock)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items");
            throw;
        }
    }

    public async Task<List<StockItem>> GetOutOfStockItemsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<StockItem>()
                .Where(x => x.IsOutOfStock && x.TrackInventory)
                .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
                .OrderBy(x => x.AvailableStock)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving out of stock items");
            throw;
        }
    }

    public async Task<StockReservation?> GetReservationAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<StockReservation>()
                .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<List<StockReservation>> GetExpiredReservationsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<StockReservation>()
                .Where(x => x.Status == ReservationStatus.Pending && x.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expired reservations");
            throw;
        }
    }

    public async Task<bool> HasSufficientStockAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stockItem = await GetByProductIdAsync(productId, cancellationToken);

            if (stockItem == null || !stockItem.TrackInventory)
                return false;

            return stockItem.AvailableStock >= quantity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking stock availability for product {ProductId}, quantity {Quantity}",
                productId,
                quantity);
            throw;
        }
    }

    public async Task<Dictionary<Guid, bool>> CheckStockAvailabilityAsync(
        Dictionary<Guid, int> productQuantities,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new Dictionary<Guid, bool>();
            var productIds = productQuantities.Keys.ToList();

            var stockItems = await _context.Set<StockItem>()
                .Where(x => productIds.Contains(x.ProductId))
                .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
                .ToListAsync(cancellationToken);

            foreach (var (productId, quantity) in productQuantities)
            {
                var stockItem = stockItems.FirstOrDefault(x => x.ProductId == productId);
                result[productId] = stockItem != null &&
                                  (!stockItem.TrackInventory || stockItem.AvailableStock >= quantity);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock availability for multiple products");
            throw;
        }
    }

    public async Task ReserveStockAsync(Guid productId, Guid? variantId, int quantity, Guid id, CancellationToken ct)
    {
        try
        {
            var stockItem = await GetByProductIdAsync(productId, ct);
            if (stockItem == null || !stockItem.TrackInventory)
                return;
            stockItem.ReserveStock( quantity, id);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Reserved {Quantity} stock for product {ProductId} ({VariantId}) for order {OrderId}",
                quantity,
                productId,
                variantId,
                id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error reserving stock for product {ProductId} ({VariantId}) for order {OrderId}",
                productId,
                variantId,
                id);
            throw;
        }
    }

    public override async Task<StockItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<StockItem>()
                .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock item {Id}", id);
            throw;
        }
    }

    // Helper method to handle reservation cleanup
    public async Task CleanupExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredReservations = await GetExpiredReservationsAsync(cancellationToken);

            foreach (var reservation in expiredReservations)
            {
                var stockItem = await GetByIdAsync(reservation.StockItemId, cancellationToken);
                if (stockItem != null)
                {
                    stockItem.CancelReservation(reservation.OrderId);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleaned up {Count} expired reservations",
                expiredReservations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired reservations");
            throw;
        }
    }
}
