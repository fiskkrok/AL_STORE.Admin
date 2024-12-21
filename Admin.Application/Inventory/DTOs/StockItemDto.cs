namespace Admin.Application.Inventory.DTOs;
public record StockItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int CurrentStock { get; init; }
    public int ReservedStock { get; init; }
    public int AvailableStock { get; init; }
    public int LowStockThreshold { get; init; }
    public bool TrackInventory { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public List<StockReservationDto> Reservations { get; init; } = new();
}
