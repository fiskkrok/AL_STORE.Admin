using Admin.Application.Orders;
using Admin.Domain.Entities;
using Admin.Domain.Enums;

namespace Admin.Application.Common.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersAsync(OrderFilterRequest filter, CancellationToken cancellationToken = default);
}