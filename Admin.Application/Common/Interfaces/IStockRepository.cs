using Admin.Domain.Entities;

namespace Admin.Application.Common.Interfaces;
public interface IStockRepository : IRepository<StockItem>
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<List<StockItem>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    Task<List<StockItem>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default);
    Task<StockReservation?> GetReservationAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<StockReservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasSufficientStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> CheckStockAvailabilityAsync(Dictionary<Guid, int> productQuantities, CancellationToken cancellationToken = default);
    Task ReserveStockAsync(Guid productId, Guid? variantId, int quantity, Guid id, CancellationToken ct);
}
