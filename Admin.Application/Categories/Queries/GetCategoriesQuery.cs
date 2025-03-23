using Admin.Application.Categories.DTOs;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using MediatR;

using Microsoft.Extensions.Caching.Hybrid;

namespace Admin.Application.Categories.Queries;
public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{

}


public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly HybridCache _cache;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, HybridCache cache)
    {
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var entryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(24),
            LocalCacheExpiration = TimeSpan.FromMinutes(30)
        };

        var categories = await _cache.GetOrCreateAsync(
            "all-categories",
            async (ct) =>
            {
                var cats = await _categoryRepository.GetAllAsync(true, ct);
                return cats.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                }).ToList();
            },
            entryOptions,
            new[] { "categories" },
            cancellationToken);

        return Result<List<CategoryDto>>.Success(categories);
    
}
}
