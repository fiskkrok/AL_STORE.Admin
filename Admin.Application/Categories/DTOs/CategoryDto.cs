// Admin.Application/Categories/DTOs/CategoryDto.cs
using System.Text.Json.Serialization;

namespace Admin.Application.Categories.DTOs;

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? ImageUrl { get; init; }
    public Guid? ParentCategoryId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CategoryDto? ParentCategory { get; init; }
    [JsonIgnore]
    public List<CategoryDto> SubCategories { get; init; } = new();
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}