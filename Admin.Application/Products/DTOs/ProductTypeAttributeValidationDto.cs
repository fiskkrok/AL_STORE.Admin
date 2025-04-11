// Admin.Application/Products/DTOs/ProductTypeAttributeValidationDto.cs

namespace Admin.Application.Products.DTOs;

public record ProductTypeAttributeValidationDto
{
    public int? Min { get; init; }
    public int? Max { get; init; }
    public string? Pattern { get; init; }
    public string? Message { get; init; }
}