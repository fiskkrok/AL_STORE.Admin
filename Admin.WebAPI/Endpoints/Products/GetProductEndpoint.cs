using Admin.Application.Products.Queries;
using Admin.Application.Products.DTOs;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Products;

public record GetProductRequest
{
    public Guid Id { get; init; }
}

public class GetProductEndpoint : Endpoint<GetProductRequest, ProductDto>
{
    private readonly IMediator _mediator;

    public GetProductEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/products/{Id}");
        Description(d => d
            .WithTags("Products")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi());
    }

    public override async Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        var query = new GetProductQuery(req.Id);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}