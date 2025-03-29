using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;

using AutoMapper;

using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Queries;
public record GetProductsQuery : IRequest<Result<PagedList<ProductDto>>>
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStock { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Status { get; set; }
    public string Visibility { get; set; }
}



public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper, ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedList<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var filterRequest = new ProductFilterRequest
            {
                SearchTerm = request.SearchTerm,
                CategoryId = request.CategoryId,
                SubCategoryId = request.SubCategoryId,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                InStock = request.InStock,
                SortBy = request.SortBy,
                SortDescending = request.SortDescending,
                PageNumber = request.Page,
                PageSize = request.PageSize
            };

            var (products, totalCount) = await _productRepository.GetProductsAsync(filterRequest, cancellationToken);
            // Log before mapping
            _logger.LogInformation("Products before mapping: {@Products}",
                products.Select(p => new { p.Id, p.Name }));
            var dtos = products.Select(p => _mapper.Map<ProductDto>(p)).ToList();
            _logger.LogInformation("Products after mapping: {@Products}",
                dtos.Select(p => new { p.Id, p.Name }));
            var pagedList = new PagedList<ProductDto>(
                dtos,
                totalCount,
                request.Page,
                request.PageSize);

            return Result<PagedList<ProductDto>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            // Log the exception here
            return Result<PagedList<ProductDto>>.Failure(
                new Error("Products.GetFailed", "Failed to retrieve products."));
        }
    }
}
