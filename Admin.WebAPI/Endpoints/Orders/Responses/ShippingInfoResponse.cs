using Admin.Application.Orders.DTOs;

namespace Admin.WebAPI.Endpoints.Orders.Responses;

public record ShippingInfoResponse(ShippingInfoDto ShippingInfo)
{
    public string Carrier => ShippingInfo.Carrier;
    public string TrackingNumber => ShippingInfo.TrackingNumber;
    public DateTime EstimatedDeliveryDate => ShippingInfo.EstimatedDeliveryDate;
    public DateTime? ActualDeliveryDate => ShippingInfo.ActualDeliveryDate;
}
