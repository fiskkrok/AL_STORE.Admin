using Admin.Application.Categories.Queries;
using Admin.Application.Categories.DTOs;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Categories;

public record GetCategoryRequest
{
    public Guid Id { get; init; }
}

public class GetCategoryEndpoint : Endpoint<GetCategoryRequest, CategoryDto>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetCategoryEndpoint> _logger;

    public GetCategoryEndpoint(IMediator mediator, ILogger<GetCategoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/categories/{Id}");
        Description(d => d
            .WithTags("Categories")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetCategory")
            .WithOpenApi());
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetCategoryRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new GetCategoryQuery(req.Id), ct);

            if (result.IsSuccess)
            {
                await SendOkAsync(result.Value, ct);
            }
            else
            {
                await SendNotFoundAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", req.Id);
            await SendErrorsAsync(500, ct);
        }
    }
}