using Admin.Application.Common.Interfaces;
using Admin.Application.Orders;
using Admin.Domain.Entities;
using Admin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Persistence.Repositories;
public class OrderRepository : Repository<Order>, IOrderRepository
{
    private readonly AdminDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(
        AdminDbContext context,
        ILogger<OrderRepository> logger) : base(context, logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<Order>()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order with ID: {OrderId}", id);
            throw;
        }
    }

    public async Task<Order?> GetByOrderNumberAsync(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<Order>()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.IsActive,
                    cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order with number: {OrderNumber}", orderNumber);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<Order>()
                .Include(o => o.Items)
                .Where(o => o.CustomerId == customerId && o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer: {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<Order>()
                .Include(o => o.Items)
                .Where(o => o.Status == status && o.IsActive)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders with status: {Status}", status);
            throw;
        }
    }


    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersAsync(
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Set<Order>()
                .Include(o => o.Items)
                .Where(o => o.IsActive)
                .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, filter);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders with filter");
            throw;
        }
    }

    private static IQueryable<Order> ApplyFilters(IQueryable<Order> query, OrderFilterRequest filter)
    {
        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId);

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate);

        if (filter.MinTotal.HasValue)
            query = query.Where(o => o.Total.Amount >= filter.MinTotal);

        if (filter.MaxTotal.HasValue)
            query = query.Where(o => o.Total.Amount <= filter.MaxTotal);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(searchTerm) ||
                o.ShippingAddress.Street.ToLower().Contains(searchTerm) ||
                o.BillingAddress.Street.ToLower().Contains(searchTerm));
        }

        return query;
    }

    private static IQueryable<Order> ApplySorting(IQueryable<Order> query, OrderFilterRequest filter)
    {
        return filter.SortBy?.ToLower() switch
        {
            "ordernumber" => filter.SortDescending
                ? query.OrderByDescending(o => o.OrderNumber)
                : query.OrderBy(o => o.OrderNumber),

            "status" => filter.SortDescending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),

            "total" => filter.SortDescending
                ? query.OrderByDescending(o => o.Total.Amount)
                : query.OrderBy(o => o.Total.Amount),

            "createdat" or _ => filter.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt)
        };
    }
}
