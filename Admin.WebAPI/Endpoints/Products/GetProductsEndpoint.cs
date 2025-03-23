using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Application.Products.Queries;

using FastEndpoints;
using MediatR;

public record GetProductsRequest
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStock { get; init; }
    public string? Status { get; init; }
    public string? Visibility { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetProductsEndpoint : EndpointWithoutRequest<PagedList<ProductDto>>
{
    private readonly IMediator _mediator;

    public GetProductsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
        Description(d => d
            .WithTags("Products")
            .WithOpenApi()
            .Produces<PagedList<ProductDto>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var request = new GetProductsQuery
        {
            SearchTerm = Query<string>("search", isRequired: false),
            CategoryId = Query<Guid?>("categoryId", isRequired: false),
            SubCategoryId = Query<Guid?>("subCategoryId", isRequired: false),
            MinPrice = Query<decimal?>("minPrice", isRequired: false),
            MaxPrice = Query<decimal?>("maxPrice", isRequired: false),
            InStock = Query<bool?>("inStock", isRequired: false),
            Status = Query<string>("status", isRequired: false),
            Visibility = Query<string>("visibility", isRequired: false),
            SortBy = Query<string>("sortBy", isRequired: false) ?? "name",
            SortDescending = Query<bool>("sortDescending", isRequired: false),
            Page = Query<int>("page", isRequired: false) <= 0 ? 1 : Query<int>("page", isRequired: false),
            PageSize = Query<int>("pageSize", isRequired: false) <= 0 ? 10 : Query<int>("pageSize", isRequired: false)
        };

        var result = await _mediator.Send(request, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendErrorsAsync(400, ct);
        }
    }
}