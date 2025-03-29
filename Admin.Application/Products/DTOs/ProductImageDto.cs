namespace Admin.Application.Products.DTOs;
public record ProductImageDto
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long Size { get; init; }
    public bool IsPrimary { get; init; }
    public int SortOrder { get; init; }
    public string? Alt { get; init; }
}
