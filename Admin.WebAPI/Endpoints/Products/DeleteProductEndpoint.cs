using Admin.Application.Products.Commands.DeleteProduct;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Admin.WebAPI.Endpoints.Products;

public record DeleteProductRequest
{
    public Guid Id { get; init; }
}

public class DeleteProductEndpoint : Endpoint<DeleteProductRequest, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteProductEndpoint> _logger;

    public DeleteProductEndpoint(IMediator mediator, ILogger<DeleteProductEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/products/{Id}");
        Description(d => d
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteProduct")
            .WithOpenApi());
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Policies(AuthConstants.CanManageProductsPolicy);
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        try
        {
            var command = new DeleteProductCommand(req.Id);
            var result = await _mediator.Send(command, ct);

            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                await SendNotFoundAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}