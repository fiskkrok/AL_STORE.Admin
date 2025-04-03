using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using Admin.Application.Inventory.Queries;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Stock;

public record GetStockItemRequest
{
    public Guid Id { get; init; }
}

public class GetStockItemEndpoint : Endpoint<GetStockItemRequest, StockItemDto>
{
    private readonly IMediator _mediator;

    public GetStockItemEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/stock/{Id}");
        Description(d => d
            .WithTags("Stock")
            .Produces<StockItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi());
        AuthSchemes(AuthConstants.JwtBearerScheme, AuthConstants.ApiKeyScheme);
        Policies(AuthConstants.CanReadProductsPolicy);
    }

    public override async Task HandleAsync(GetStockItemRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStockItemQuery(req.Id), ct);
        
        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }
            
        await SendOkAsync(result.Value, ct);
    }
}