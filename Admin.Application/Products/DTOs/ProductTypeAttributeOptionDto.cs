namespace Admin.Application.Products.DTOs;

public record ProductTypeAttributeOptionDto
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}