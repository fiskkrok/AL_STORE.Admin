using System.Text.Json.Serialization;

using Admin.Application.Categories.DTOs;
using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using Admin.Application.Orders.DTOs;
using Admin.Application.Products.DTOs;
using Admin.WebAPI.Endpoints.Dashboard;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(CategoryDto))]
[JsonSerializable(typeof(List<CategoryDto>))]
[JsonSerializable(typeof(ProductDto))]
[JsonSerializable(typeof(List<ProductDto>))]
[JsonSerializable(typeof(ProductVariantDto))]
[JsonSerializable(typeof(List<ProductVariantDto>))]
[JsonSerializable(typeof(OrderDto))]
[JsonSerializable(typeof(DashboardStats))]
[JsonSerializable(typeof(List<OrderDto>))]
[JsonSerializable(typeof(StockItemDto))]
[JsonSerializable(typeof(List<StockItemDto>))]
[JsonSerializable(typeof(PagedList<ProductDto>))]
[JsonSerializable(typeof(PagedList<OrderDto>))]
[JsonSerializable(typeof(PagedList<CategoryDto>))]
[JsonSerializable(typeof(Result<ProductDto>))]
[JsonSerializable(typeof(Result<OrderDto>))]
[JsonSerializable(typeof(Result<CategoryDto>))]
[JsonSerializable(typeof(Result<DashboardStats>))]
[JsonSerializable(typeof(Result<List<ProductDto>>))]
[JsonSerializable(typeof(Result<List<OrderDto>>))]
[JsonSerializable(typeof(Result<List<CategoryDto>>))]
[JsonSerializable(typeof(Result<PagedList<ProductDto>>))]
[JsonSerializable(typeof(Result<PagedList<OrderDto>>))]
[JsonSerializable(typeof(Result<PagedList<CategoryDto>>))]
[JsonSerializable(typeof(Error))]
[JsonSerializable(typeof(ErrorResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}