// Admin.WebAPI/Endpoints/Products/GetProductTypeByIdEndpoint.cs
using Admin.Application.Products.DTOs;
using Admin.Application.Products.Queries;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Products;

public class GetProductTypeByIdEndpoint : Endpoint<GetProductTypeByIdRequest, IResult>
{
    private readonly IMediator _mediator;

    public GetProductTypeByIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/products/types/{Id}");
        AllowAnonymous(); // Or add appropriate authorization
        Description(d => d
            .WithTags("ProductTypes")
            .Produces<ProductTypeDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi());
    }

    public override async Task HandleAsync(GetProductTypeByIdRequest req, CancellationToken ct)
    {
        var query = new GetProductTypeByIdQuery(req.Id);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess && result.Value is not null)
        {
            await SendAsync(Results.Ok(result.Value), cancellation: ct); // Use Results.Ok to create an IResult
        }
        else
        {
            await SendAsync(Results.NotFound(), cancellation: ct); // Use Results.NotFound to create an IResult
        }
    }
}

public record GetProductTypeByIdRequest
{
    public string Id { get; init; } = string.Empty;
}
