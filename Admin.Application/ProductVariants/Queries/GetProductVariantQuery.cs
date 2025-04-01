using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using AutoMapper;
using MediatR;

namespace Admin.Application.ProductVariants.Queries;
public record GetProductVariantQuery(Guid ProductId, Guid VariantId) : IRequest<Result<ProductVariantDto>>;

public class GetProductVariantQueryHandler : IRequestHandler<GetProductVariantQuery, Result<ProductVariantDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductVariantQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProductVariantDto>> Handle(GetProductVariantQuery request, CancellationToken cancellationToken)
    {
        var variant = await _productRepository.GetVariantByIdAsync(request.VariantId, cancellationToken);

        if (variant == null || variant.ProductId != request.ProductId)
            return Result<ProductVariantDto>.Failure(new Error("Variant.NotFound", "Product variant not found"));

        var dto = _mapper.Map<ProductVariantDto>(variant);

        // Ensure collections are initialized
        dto = dto with
        {
            Attributes = dto.Attributes ?? [],
            Images = dto.Images ?? []
        };

        return Result<ProductVariantDto>.Success(dto);
    }
}
