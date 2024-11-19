using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using AutoMapper;

using MediatR;

namespace Admin.Application.Products.Queries;

public record GetProductQuery(Guid Id) : IRequest<Result<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
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

            var dtos = products.Select(p => _mapper.Map<ProductDto>(p)).ToList();

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