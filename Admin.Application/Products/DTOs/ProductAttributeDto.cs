namespace Admin.Application.Products.DTOs;
public record ProductAttributeDto
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}