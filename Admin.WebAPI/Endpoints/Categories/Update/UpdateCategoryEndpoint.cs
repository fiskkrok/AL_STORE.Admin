using Admin.Application.Categories.Commands;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Categories.Update;

public class UpdateCategoryEndpoint : Endpoint<UpdateCategoryCommand, IResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateCategoryEndpoint> _logger;

    public UpdateCategoryEndpoint(IMediator mediator, ILogger<UpdateCategoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/categories/{id}");
        Description(d => d
            .WithTags("Categories")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateCategory")
            .WithOpenApi());
        Policies("ProductEdit", "FullAdminAccess");
    }

    public override async Task HandleAsync(UpdateCategoryCommand req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

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
            _logger.LogError(ex, "Error updating category {CategoryId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}
