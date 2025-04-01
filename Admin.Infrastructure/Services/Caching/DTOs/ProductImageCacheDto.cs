namespace Admin.Infrastructure.Services.Caching.DTOs;

public class ProductImageCacheDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}