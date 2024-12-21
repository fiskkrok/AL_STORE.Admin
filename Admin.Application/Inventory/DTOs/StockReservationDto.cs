using Admin.Domain.Entities;

namespace Admin.Application.Inventory.DTOs;
public record StockReservationDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public int Quantity { get; init; }
    public ReservationStatus Status { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
}