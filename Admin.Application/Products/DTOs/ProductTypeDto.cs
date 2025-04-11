// Admin.Application/Products/DTOs/ProductTypeDto.cs
namespace Admin.Application.Products.DTOs;

public record ProductTypeDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public List<ProductTypeAttributeDto> Attributes { get; set; } = new();
}