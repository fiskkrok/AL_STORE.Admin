using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using AutoMapper;

using MediatR;

namespace Admin.Application.Products.Queries;

public record GetProductQuery(Guid Id) : IRequest<Result<ProductDto>>;

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

            if (product == null)
            {
                return Result<ProductDto>.Failure(
                    new Error("Product.NotFound", $"Product with ID {request.Id} was not found"));
            }

            var dto = _mapper.Map<ProductDto>(product);
            return Result<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            // Log the exception here
            return Result<ProductDto>.Failure(
                new Error("Product.GetFailed", "Failed to retrieve product"));
        }
    }
}