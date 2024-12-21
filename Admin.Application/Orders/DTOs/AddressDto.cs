namespace Admin.Application.Orders.DTOs;
public record AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
}
