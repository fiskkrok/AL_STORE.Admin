namespace Admin.Application.Products.DTOs;

public record ProductTypeAttributeDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public bool IsRequired { get; init; }
    public object? DefaultValue { get; init; }
    public List<ProductTypeAttributeOptionDto>? Options { get; init; }
    public ProductTypeAttributeValidationDto? Validation { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsFilterable { get; init; }
    public bool IsComparable { get; init; }
}