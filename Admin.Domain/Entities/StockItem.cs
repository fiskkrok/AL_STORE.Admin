using Admin.Domain.Common;
using Admin.Domain.Common.Exceptions;
using Admin.Domain.Events.Stock;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
public class StockItem : AuditableEntity
{
    private int _currentStock;
    private int _reservedStock;
    private int _lowStockThreshold;
    private bool _trackInventory = true;
    private StockItem() { } // Required by EF Core

    public StockItem(
        Guid productId,
        int initialStock,
        int lowStockThreshold,
        bool trackInventory = true)
    {
        Guard.Against.NegativeOrZero(initialStock, nameof(initialStock));
        Guard.Against.Negative(lowStockThreshold, nameof(lowStockThreshold));

        ProductId = productId;
        _currentStock = initialStock;
        _lowStockThreshold = lowStockThreshold;
        _trackInventory = trackInventory;

        AddDomainEvent(new StockItemCreatedDomainEvent(this));
    }

    public Guid ProductId { get; private set; }
    public int CurrentStock => _currentStock;
    public int ReservedStock => _reservedStock;
    public int AvailableStock
    {
        get => _currentStock - _reservedStock;
        private set { } // Adding a private setter to satisfy EF Core
    }
    public int LowStockThreshold => _lowStockThreshold;
    public bool TrackInventory => _trackInventory;
    public bool IsLowStock
    {
        get => AvailableStock <= _lowStockThreshold;
        private set { } // Adding a private setter to satisfy EF Core
    }

    public bool IsOutOfStock
    {
        get => AvailableStock <= 0;
        private set { } // Adding a private setter to satisfy EF Core
    }

    public StockReservation ReserveStock(int quantity, Guid orderId)
    {
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));

        if (!_trackInventory)
            return new StockReservation(Id, orderId, quantity, ReservationStatus.Confirmed);

        if (quantity > AvailableStock)
            throw new InsufficientStockException(ProductId, quantity, AvailableStock);

        _reservedStock += quantity;

        var reservation = new StockReservation(Id, orderId, quantity, ReservationStatus.Pending);

        AddDomainEvent(new StockReservedDomainEvent(this, orderId, quantity));

        if (IsLowStock)
            AddDomainEvent(new LowStockDomainEvent(this));

        return reservation;
    }

    public void CommitReservation(Guid orderId)
    {
        if (!_trackInventory) return;

        var reservation = Reservations.FirstOrDefault(r => r.OrderId == orderId && r.Status == ReservationStatus.Pending)
            ?? throw new ReservationNotFoundException(orderId);

        _currentStock -= reservation.Quantity;
        _reservedStock -= reservation.Quantity;

        reservation.Confirm();

        AddDomainEvent(new StockCommittedDomainEvent(this, orderId, reservation.Quantity));
    }

    public void CancelReservation(Guid orderId)
    {
        if (!_trackInventory) return;

        var reservation = Reservations.FirstOrDefault(r => r.OrderId == orderId && r.Status == ReservationStatus.Pending)
            ?? throw new ReservationNotFoundException(orderId);

        _reservedStock -= reservation.Quantity;

        reservation.Cancel();

        AddDomainEvent(new StockReservationCancelledDomainEvent(this, orderId, reservation.Quantity));
    }

    public void AdjustStock(int adjustment, string reason, string? modifiedBy = null)
    {
        if (!_trackInventory) return;

        Guard.Against.Null(reason, nameof(reason));

        var oldStock = _currentStock;
        _currentStock += adjustment;

        if (_currentStock < 0)
            _currentStock = 0;

        AddDomainEvent(new StockAdjustedDomainEvent(this, oldStock, _currentStock, reason));

        if (IsLowStock)
            AddDomainEvent(new LowStockDomainEvent(this));

        SetModified(modifiedBy);
    }

    public void UpdateLowStockThreshold(int newThreshold, string? modifiedBy = null)
    {
        Guard.Against.Negative(newThreshold, nameof(newThreshold));

        _lowStockThreshold = newThreshold;

        if (IsLowStock)
            AddDomainEvent(new LowStockDomainEvent(this));

        SetModified(modifiedBy);
    }

    public void SetTrackInventory(bool trackInventory, string? modifiedBy = null)
    {
        _trackInventory = trackInventory;
        SetModified(modifiedBy);
    }

    private readonly List<StockReservation> _reservations = new();
    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();
}