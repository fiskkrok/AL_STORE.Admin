using Admin.Domain.ValueObjects;

namespace Admin.Application.Orders.DTOs;
public record ShippingInfoDto
{
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public DateTime EstimatedDeliveryDate { get; init; }
    public DateTime? ActualDeliveryDate { get; init; }

    public Address ToEntity()
    {
        throw new NotImplementedException();
    }
}
