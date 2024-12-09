namespace Admin.Application.Products.DTOs;
public record ProductSeoDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public List<string> Keywords { get; init; } = new();
}
