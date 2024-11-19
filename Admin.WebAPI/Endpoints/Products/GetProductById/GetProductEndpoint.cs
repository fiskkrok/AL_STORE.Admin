using Admin.Application.Products.Queries;
using FastEndpoints;
using MediatR;
using Admin.WebAPI.Models.Responses;

namespace Admin.WebAPI.Endpoints.Products.GetProductById;

public class GetProductEndpoint : Endpoint<GetProductByIdRequest, ProductDetailsResponse>
{

    private readonly IMediator _mediator;
    private readonly ILogger<GetProductEndpoint> _logger;

    public GetProductEndpoint(IMediator mediator, ILogger<GetProductEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/products/{Id}");
        Description(d => d
            .WithTags("Products")
            .Produces<ProductDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetProductById")
            .WithOpenApi());
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var query = new GetProductQuery(req.Id);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            var response = ProductDetailsResponse.FromDto(result.Value);
            await SendOkAsync(response, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }



}