using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Application.Products.Queries;
using AutoMapper;
using MediatR;

public record GetAllProductsQuery : IRequest<Result<BulkProductsResponse>>
{
    public DateTime? Since { get; init; }
}

public record BulkProductsResponse
{
    public string BatchId { get; init; } = Guid.NewGuid().ToString();
    public List<ProductDto> Products { get; init; } = new();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<BulkProductsResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<BulkProductsResponse>> Handle(GetAllProductsQuery request, CancellationToken ct)
    {
        var filter = new ProductFilterRequest
        {
            PageSize = int.MaxValue,
            LastModifiedAfter = request.Since
        };

        var (products, _) = await _productRepository.GetProductsAsync(filter, ct);

        var response = new BulkProductsResponse
        {
            Products = products.Select(p => _mapper.Map<ProductDto>(p)).ToList()
        };

        return Result<BulkProductsResponse>.Success(response);
    }
}