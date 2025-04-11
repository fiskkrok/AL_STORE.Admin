// Admin.Application/Products/Queries/GetProductTypeByIdQuery.cs
using System.Text.Json;

using Admin.Application.Common.CQRS;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Queries;

public record GetProductTypeByIdQuery(string Id) : IQuery<ProductTypeDto>;

public class GetProductTypeByIdQueryHandler : QueryHandler<GetProductTypeByIdQuery, ProductTypeDto>
{
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetProductTypeByIdQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<GetProductTypeByIdQueryHandler> logger) : base(context, logger)
    {
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public override async Task<Result<ProductTypeDto>> Handle(
        GetProductTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try cache first
            var cacheKey = $"product:type:{request.Id}";
            var cached = await _cacheService.GetAsync<ProductTypeDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return Result<ProductTypeDto>.Success(cached);
            }

            // Get from database
            var productType = await Context.ProductTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == new Guid(request.Id), cancellationToken);

            if (productType == null)
            {
                return Result<ProductTypeDto>.Failure(
                    new Error("ProductType.NotFound", $"Product type with ID {request.Id} was not found"));
            }

            // Map to DTO
            var dto = _mapper.Map<ProductTypeDto>(productType);
            dto.Attributes = JsonSerializer.Deserialize<List<ProductTypeAttributeDto>>(productType.AttributesJson) ?? new List<ProductTypeAttributeDto>();

            // Cache result
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromHours(1), cancellationToken);

            return Result<ProductTypeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving product type {ProductTypeId}", request.Id);
            return Result<ProductTypeDto>.Failure(
                new Error("ProductType.GetFailed", "Failed to retrieve product type"));
        }
    }
}