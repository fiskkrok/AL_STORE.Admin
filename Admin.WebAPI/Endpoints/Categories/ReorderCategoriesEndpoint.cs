using Admin.Application.Categories.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Categories;

public record ReorderCategoryRequest
{
    public Guid CategoryId { get; init; }
    public int NewSortOrder { get; init; }
}

public class ReorderCategoriesEndpoint : Endpoint<List<ReorderCategoryRequest>, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReorderCategoriesEndpoint> _logger;

    public ReorderCategoriesEndpoint(IMediator mediator, ILogger<ReorderCategoriesEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/categories/reorder");
        Description(d => d
            .WithTags("Categories")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("ReorderCategories")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(List<ReorderCategoryRequest> req, CancellationToken ct)
    {
        try
        {
            foreach (var item in req)
            {
                var command = new UpdateCategoryCommand
                {
                    Id = item.CategoryId,
                    SortOrder = item.NewSortOrder
                };

                var result = await _mediator.Send(command, ct);
                if (!result.IsSuccess)
                {
                    await SendErrorsAsync(400, ct);
                    return;
                }
            }

            await SendNoContentAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering categories");
            await SendErrorsAsync(500, ct);
        }
    }
}