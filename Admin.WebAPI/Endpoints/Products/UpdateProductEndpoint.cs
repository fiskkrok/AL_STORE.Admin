using System.Globalization;

using Admin.Application.Products.Commands.UpdateProduct;
using Admin.WebAPI.Endpoints.Products.Request;
using Admin.WebAPI.Infrastructure.Authorization;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Products;
[RequirePermission(Domain.Constants.Permissions.Products.Manage)]
public class UpdateProductEndpoint : Endpoint<UpdateProductRequest, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateProductEndpoint> _logger;

    public UpdateProductEndpoint(IMediator mediator, ILogger<UpdateProductEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/products/{Id}");
        Description(d => d
            .WithTags("Products")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("UpdateProduct")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        try
        {
            var command = new UpdateProductCommand
            {
                Id = req.Id,
                Name = req.Name,
                Description = req.Description,
                Price = decimal.Parse(req.Price.ToString(), CultureInfo.InvariantCulture),
                Currency = req.Currency,
                CategoryId = req.CategoryId,
                SubCategoryId = req.SubCategoryId.Equals(Guid.Empty) ? req.CategoryId : req.SubCategoryId,
                NewImages = req.NewImages ?? [],
                ImageIdsToRemove = req.ImageIdsToRemove ?? []
            };

            var result = await _mediator.Send(command, ct);
            if (result.IsSuccess)
            {
                await SendNoContentAsync(ct);
            }
            else
            {
                await SendErrorsAsync(400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}
