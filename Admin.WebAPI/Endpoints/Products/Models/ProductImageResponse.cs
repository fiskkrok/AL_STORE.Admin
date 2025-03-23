using Admin.Application.Products.DTOs;

namespace Admin.WebAPI.Endpoints.Products.Models;

public record ProductImageResponse
{
    public Guid Id { get; init; }
    public string Url { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long Size { get; init; }

    public static ProductImageResponse FromDto(ProductImageDto dto) =>
        new()
        {
            Id = dto.Id,
            Url = dto.Url,
            FileName = dto.FileName,
            Size = dto.Size
        };
}
