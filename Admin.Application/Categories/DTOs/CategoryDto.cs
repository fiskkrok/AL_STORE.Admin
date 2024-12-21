namespace Admin.Application.Categories.DTOs;
// DTOs
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public CategoryDto? ParentCategory { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
