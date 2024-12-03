using Admin.Application.Products.DTOs;

namespace Admin.WebAPI.Endpoints.Products.Responses;

public record CategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public static CategoryResponse FromDto(CategoryDto dto) =>
        new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
}
