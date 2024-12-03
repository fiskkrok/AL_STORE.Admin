using Admin.Application.Products.Commands.CreateProduct;
using Admin.WebAPI.Endpoints.Products.GetProductById;
using FastEndpoints;

using MediatR;
using Microsoft.Extensions.Caching.Hybrid;

namespace Admin.WebAPI.Endpoints.Products.CreateProduct;

public class CreateProductEndpoint(IMediator mediator, ILogger<CreateProductEndpoint> logger, HybridCache cache)
    : Endpoint<CreateProductCommand, Guid>
{
    public override void Configure()
    {
        Post("/products");
        Description(d => d
            .WithTags("Products")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithOpenApi());
        Policies("ProductCreate", "FullAdminAccess");
    }

    public override async Task HandleAsync(CreateProductCommand req, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Received create product request: {@Request}", req);

            var result = await mediator.Send(req, ct);
            if (result.IsSuccess)
            {
                await cache.RemoveByTagAsync("products", ct);
                await SendCreatedAtAsync<GetProductEndpoint>(
                    new { id = result.Value },
                    result.Value,
                    generateAbsoluteUrl: true,
                    cancellation: ct);
            }
            else
            {
                logger.LogWarning("Failed to create product: {@Result}", result);
                await SendErrorsAsync(400, cancellation: ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product");
            await SendErrorsAsync(500, ct);
        }
    }
}