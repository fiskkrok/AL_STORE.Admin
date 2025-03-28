using Admin.Application.Common.CQRS;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Queries;

// Query definition
public record GetProductQuery(Guid Id) : IQuery<ProductDto>;

// Query handler
public class GetProductQueryHandler : QueryHandler<GetProductQuery, ProductDto>
{
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly IApplicationDbContext _dbContext;

    public GetProductQueryHandler(
        IApplicationDbContext dbContext,
        IMapper mapper,
        ICacheService cacheService,
        ILogger<GetProductQueryHandler> logger) : base(dbContext, logger)
    {
        _mapper = mapper;
        _cacheService = cacheService;
        _dbContext = dbContext;
    }


    public override async Task<Result<ProductDto>> Handle(
        GetProductQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = $"product:{query.Id}";
            var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);

            if (cachedProduct != null)
            {
                Logger.LogDebug("Cache hit for product {ProductId}", query.Id);
                return Result<ProductDto>.Success(cachedProduct);
            }
            
            // Cache miss, query the database
            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Include(p => p.Attributes)
                .AsNoTracking() // Important for read operations
                //.AsSplitQuery() 
                .FirstOrDefaultAsync(p => p.Id == query.Id && !p.IsArchived, cancellationToken);

            if (product == null)
            {
                return Result<ProductDto>.Failure(
                    new Error("Product.NotFound", $"Product with ID {query.Id} was not found"));
            }

            // Map to DTO
            var dto = _mapper.Map<ProductDto>(product);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);

            return Result<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving product {ProductId}", query.Id);
            return Result<ProductDto>.Failure(
                new Error("Product.GetFailed", "Failed to retrieve product"));
        }
    }
}