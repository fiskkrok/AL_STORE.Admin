using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.StoreAPI;

public class GetAllProductsEndpoint : EndpointWithoutRequest<BulkProductsResponse>
{
    private readonly IMediator _mediator;

    public GetAllProductsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/products/bulk");
        Description(d => d
            .WithTags("Products")
            .Produces<BulkProductsResponse>(200)
            .WithName("GetAllProducts")
            .WithOpenApi());
        AuthSchemes(AuthConstants.ApiKeyScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var since = Query<DateTime?>("since", isRequired: false);
        var query = new GetAllProductsQuery { Since = since };
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
            await SendOkAsync(result.Value, ct);
        else
            await SendErrorsAsync(400, ct);
    }
}
