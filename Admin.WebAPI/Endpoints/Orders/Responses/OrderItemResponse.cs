using Admin.Application.Orders.DTOs;

namespace Admin.WebAPI.Endpoints.Orders.Responses;

public record OrderItemResponse(OrderItemDto Item)
{
    public Guid Id => Item.Id;
    public Guid ProductId => Item.ProductId;
    public int Quantity => Item.Quantity;
    public decimal UnitPrice => Item.UnitPrice;
    public string Currency => Item.Currency;
    public decimal Total => Item.Total;
}