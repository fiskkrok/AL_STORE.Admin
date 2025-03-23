using Admin.Application.Categories.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Categories;

public record DeleteCategoryRequest
{
    public Guid Id { get; init; }
}

public class DeleteCategoryEndpoint : Endpoint<DeleteCategoryRequest, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteCategoryEndpoint> _logger;

    public DeleteCategoryEndpoint(IMediator mediator, ILogger<DeleteCategoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/categories/{Id}");
        Description(d => d
            .WithTags("Categories")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteCategory")
            .WithOpenApi());
        Policies("ProductsDelete", "FullAdminAccess");
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        try
        {
            var command = new DeleteCategoryCommand(req.Id);
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
            _logger.LogError(ex, "Error deleting category {CategoryId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}