using Admin.Application.Orders.DTOs;

namespace Admin.WebAPI.Endpoints.Orders.Responses;

public record AddressResponse(AddressDto Address)
{
    public string Street => Address.Street;
    public string City => Address.City;
    public string State => Address.State;
    public string Country => Address.Country;
    public string PostalCode => Address.PostalCode;
}