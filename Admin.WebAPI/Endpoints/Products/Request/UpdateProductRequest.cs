using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;

namespace Admin.WebAPI.Endpoints.Products.Request;

public class UpdateProductRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; }
    public Guid CategoryId { get; init; }
    public Guid SubCategoryId { get; init; }
    public List<ProductImageDto> NewImages { get; init; }
    public List<Guid> ImageIdsToRemove { get; init; }
}
