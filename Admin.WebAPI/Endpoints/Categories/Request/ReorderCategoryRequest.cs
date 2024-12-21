namespace Admin.WebAPI.Endpoints.Categories.Request;

public record ReorderCategoryRequest
{
    public Guid CategoryId { get; init; }
    public int NewSortOrder { get; init; }
}
