using Admin.Application.Categories.DTOs;
using Admin.Application.Categories.Queries;
using Admin.WebAPI.Endpoints.Categories.Models;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Categories;

public class GetCategoryEndpoint : Endpoint<GetCategoryRequest, CategoryResponse>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetCategoryEndpoint> _logger;

    public GetCategoryEndpoint(IMediator mediator, ILogger<GetCategoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/categories/{Id}");
        Description(d => d
            .WithTags("Categories")
            .Produces<CategoryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetCategory")
            .WithOpenApi());
        AllowAnonymous(); // Categories can be viewed without authentication
    }

    public override async Task HandleAsync(GetCategoryRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetCategoryQuery(req.Id), ct);

            if (result.IsSuccess)
            {
                var response = new CategoryResponse
                {
                    Id = result.Value.Id,
                    Name = result.Value.Name,
                    Description = result.Value.Description,
                    Slug = result.Value.Slug,
                    SortOrder = result.Value.SortOrder,
                    MetaTitle = result.Value.MetaTitle,
                    MetaDescription = result.Value.MetaDescription,
                    ImageUrl = result.Value.ImageUrl,
                    ParentCategoryId = result.Value.ParentCategoryId,
                    ParentCategory = result.Value.ParentCategory != null
                        ? MapParentCategory(result.Value.ParentCategory)
                        : null,
                    SubCategories = result.Value.SubCategories
                        .Select(MapSubCategory)
                        .ToList(),
                    ProductCount = result.Value.ProductCount,
                    CreatedAt = result.Value.CreatedAt,
                    CreatedBy = result.Value.CreatedBy,
                    LastModifiedAt = result.Value.LastModifiedAt,
                    LastModifiedBy = result.Value.LastModifiedBy
                };

                await SendOkAsync(response, ct);
            }
            else
            {
                await SendNotFoundAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }

    private static CategoryResponse MapParentCategory(CategoryDto category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            SortOrder = category.SortOrder,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            ProductCount = category.ProductCount
        };
    }

    private static CategoryResponse MapSubCategory(CategoryDto category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            SortOrder = category.SortOrder,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            ProductCount = category.ProductCount
        };
    }
}

public class GetCategoryRequest
{
    public Guid Id { get; init; }
}
