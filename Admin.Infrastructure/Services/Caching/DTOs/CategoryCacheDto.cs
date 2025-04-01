namespace Admin.Infrastructure.Services.Caching.DTOs;

public class CategoryCacheDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }

    // For caching hierarchies, we only keep IDs to avoid circular references
    public List<Guid> SubCategoryIds { get; set; } = new();
}