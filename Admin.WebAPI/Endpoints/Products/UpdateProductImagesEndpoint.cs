// Admin.WebAPI/Endpoints/Products/UpdateProductImagesEndpoint.cs
using Admin.Application.Products.Commands.UpdateProductImages;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Admin.WebAPI.Endpoints.Products;

public class UpdateProductImagesEndpoint : Endpoint<UpdateProductImagesCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateProductImagesEndpoint> _logger;

    public UpdateProductImagesEndpoint(IMediator mediator, ILogger<UpdateProductImagesEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/products/{ProductId}/images");
        Description(d => d
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("UpdateProductImages")
            .WithOpenApi());
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Policies(AuthConstants.CanManageProductsPolicy);
    }

    public override async Task HandleAsync(UpdateProductImagesCommand req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else if (result.Error?.Code == "Product.NotFound")
            {
                await SendNotFoundAsync(ct);
            }
            else
            {
                await SendErrorsAsync(400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product images for product {ProductId}", req.ProductId);
            await SendErrorsAsync(500, ct);
        }
    }
}