using Admin.Application.Common.Models;
using Admin.Application.Inventory.DTOs;
using Admin.Application.Inventory.Queries;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Stock;

public class GetLowStockItemsEndpoint : EndpointWithoutRequest<List<StockItemDto>>
{
    private readonly IMediator _mediator;

    public GetLowStockItemsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/stock/low-stock");
        Description(d => d
            .WithTags("Stock")
            .Produces<List<StockItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi());
        AuthSchemes(AuthConstants.JwtBearerScheme, AuthConstants.ApiKeyScheme);
        Policies(AuthConstants.CanReadProductsPolicy);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLowStockItemsQuery(), ct);
        
        if (!result.IsSuccess)
        {
            await SendErrorsAsync(400, ct);
            return;
        }
            
        await SendOkAsync(result.Value, ct);
    }
}