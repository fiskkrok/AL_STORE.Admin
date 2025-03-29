// Admin.WebAPI/Endpoints/Products/GetProductTypesEndpoint.cs
using Admin.Application.Products.DTOs;
using Admin.Application.Products.Queries;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Products;

public class GetProductTypesEndpoint : EndpointWithoutRequest<List<ProductTypeDto>>
{
    private readonly IMediator _mediator;

    public GetProductTypesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/products/types");
        AllowAnonymous(); // Or add the appropriate authentication/authorization
        Description(d => d
            .WithTags("Products")
            .Produces<List<ProductTypeDto>>(200)
            .Produces(400)
            .WithName("GetProductTypes")
            .WithOpenApi());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetProductTypesQuery();
        var result = await _mediator.Send(query, ct);

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