using Admin.Application.Products.Commands.UpdateProduct;
using Admin.Application.Products.DTOs;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Products;

public record UpdateProductRequest
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public Guid CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public List<ProductImageDto>? NewImages { get; init; }
    public List<Guid>? ImageIdsToRemove { get; init; }
}

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
                Price = req.Price,
                Currency = req.Currency,
                CategoryId = req.CategoryId,
                SubCategoryId = req.SubCategoryId == Guid.Empty ? null : req.SubCategoryId,
                NewImages = req.NewImages ?? new List<ProductImageDto>(),
                ImageIdsToRemove = req.ImageIdsToRemove ?? new List<Guid>()
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