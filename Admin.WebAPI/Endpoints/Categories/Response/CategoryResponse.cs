namespace Admin.WebAPI.Endpoints.Categories.Response;

// Response models
public record CategoryResponse
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
    public CategoryResponse? ParentCategory { get; init; }
    public List<CategoryResponse> SubCategories { get; init; } = new();
    public int ProductCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public string? LastModifiedBy { get; init; }
}