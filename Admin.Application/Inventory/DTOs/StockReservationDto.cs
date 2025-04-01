// Admin.Application/Inventory/DTOs/StockReservationDto.cs
namespace Admin.Application.Inventory.DTOs;
public record StockReservationDto
{
    public Guid Id { get; init; }
    public Guid OrderId { get; init; }
    public int Quantity { get; init; }
    public string Status { get; init; } = string.Empty; // "Pending", "Confirmed", "Cancelled"
    public DateTime ExpiresAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
}