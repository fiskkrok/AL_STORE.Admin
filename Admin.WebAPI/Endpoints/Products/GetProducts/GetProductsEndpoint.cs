using Admin.Application.Products.Queries;
using Admin.WebAPI.Endpoints.Products.Responses;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Products.GetProducts;

public class GetProductsEndpoint : EndpointWithoutRequest<PagedResponse<ProductResponse>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetProductsEndpoint> _logger;

    public GetProductsEndpoint(IMediator mediator, ILogger<GetProductsEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
        Description(d => d
            .WithTags("Products")
            .WithOpenApi()
            .Produces<PagedResponse<ProductResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var request = new GetProductsQuery
        {
            SearchTerm = Query<string>("searchTerm", isRequired: false),
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
            var response = PagedResponse<ProductResponse>.FromPagedList(
                result.Value,
                dto => ProductResponse.FromDto(dto));

            await SendOkAsync(response, ct);
        }
        else
        {
            await SendErrorsAsync(400, ct);
        }
    }
}