using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Common;

namespace Admin.Domain.Entities;
public class StockReservation : AuditableEntity
{
    private StockReservation() { } // Required by EF Core

    internal StockReservation(
        Guid stockItemId,
        Guid orderId,
        int quantity,
        ReservationStatus status)
    {
        StockItemId = stockItemId;
        OrderId = orderId;
        Quantity = quantity;
        Status = status;
        ExpiresAt = DateTime.UtcNow.AddMinutes(30); // 30-minute reservation
    }

    public Guid StockItemId { get; private set; }
    public Guid OrderId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    internal void Confirm()
    {
        Status = ReservationStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    internal void Cancel()
    {
        Status = ReservationStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
    }

    public bool IsExpired => Status == ReservationStatus.Pending && DateTime.UtcNow > ExpiresAt;
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled
}
