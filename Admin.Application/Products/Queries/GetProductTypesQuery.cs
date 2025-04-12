// Admin.Application/Products/Queries/GetProductTypesQuery.cs
using System.Text.Json;

using Admin.Application.Common.CQRS;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;

using AutoMapper;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Queries;

public record GetProductTypesQuery : IQuery<List<ProductTypeDto>>;

public class GetProductTypesQueryHandler : QueryHandler<GetProductTypesQuery, List<ProductTypeDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetProductTypesQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<GetProductTypesQueryHandler> logger) : base(context, logger)
    {
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public override async Task<Result<List<ProductTypeDto>>> Handle(
        GetProductTypesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try cache first
            var cacheKey = "product:types:all";
            var cached = await _cacheService.GetAsync<List<ProductTypeDto>>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return Result<List<ProductTypeDto>>.Success(cached);
            }

            // Get from database
            var productTypes = await Context.ProductTypes
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var dtos = new List<ProductTypeDto>();
            foreach (var pt in productTypes)
            {
                var dto = _mapper.Map<ProductTypeDto>(pt);
                dto.Attributes = JsonSerializer.Deserialize<List<ProductTypeAttributeDto>>(pt.AttributesJson) ?? new List<ProductTypeAttributeDto>();
                dtos.Add(dto);
            }

            // Cache the result
            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromHours(1), cancellationToken);

            return Result<List<ProductTypeDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving product types");
            return Result<List<ProductTypeDto>>.Failure(
                new Error("ProductTypes.GetFailed", "Failed to retrieve product types"));
        }
    }

}