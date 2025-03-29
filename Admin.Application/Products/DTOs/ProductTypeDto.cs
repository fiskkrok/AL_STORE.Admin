// Admin.Application/Products/DTOs/ProductTypeDto.cs
namespace Admin.Application.Products.DTOs;

public record ProductTypeDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public List<ProductTypeAttributeDto> Attributes { get; init; } = new();
}

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

public record ProductTypeAttributeOptionDto
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public record ProductTypeAttributeValidationDto
{
    public int? Min { get; init; }
    public int? Max { get; init; }
    public string? Pattern { get; init; }
    public string? Message { get; init; }
}