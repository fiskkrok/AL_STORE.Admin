using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Admin.Infrastructure.Persistence.Repositories;
public class StockRepository : BaseRepository<StockItem>, IStockRepository
{
    public StockRepository(AdminDbContext context) : base(context)
    {
    }

    public override async Task<StockItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StockItem>()
            .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StockItem>()
            .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }

    public async Task<List<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StockItem>()
            .Where(x => x.IsLowStock && x.TrackInventory)
            .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
            .OrderBy(x => x.AvailableStock)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StockItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StockItem>()
            .Where(x => x.IsOutOfStock && x.TrackInventory)
            .Include(x => x.Reservations.Where(r => r.Status == ReservationStatus.Pending))
            .OrderBy(x => x.AvailableStock)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockReservation?> GetReservationAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StockReservation>()
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public async Task<List<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<StockReservation>()
            .Where(x => x.Status == ReservationStatus.Pending && x.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasSufficientStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var stockItem = await GetByProductIdAsync(productId, cancellationToken);

        if (stockItem == null || !stockItem.TrackInventory)
            return false;

        return stockItem.AvailableStock >= quantity;
    }

    public async Task<Dictionary<Guid, bool>> CheckStockAvailabilityAsync(
        Dictionary<Guid, int> productQuantities,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, bool>();
        var productIds = productQuantities.Keys.ToList();

        var stockItems = await DbContext.Set<StockItem>()
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

    public async Task ReserveStockAsync(Guid productId, Guid? variantId, int quantity, Guid id, CancellationToken ct)
    {
        var stockItem = await GetByProductIdAsync(productId, ct);
        if (stockItem == null || !stockItem.TrackInventory)
            return;

        stockItem.ReserveStock(quantity, id);
    }
}
