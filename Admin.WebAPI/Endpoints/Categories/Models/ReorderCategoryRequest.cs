namespace Admin.WebAPI.Endpoints.Categories.Models;

public record ReorderCategoryRequest
{
    public Guid CategoryId { get; init; }
    public int NewSortOrder { get; init; }
}
