using Admin.Application.Categories.Queries;
using Admin.Application.Categories.DTOs;
using FastEndpoints;
using MediatR;

namespace Admin.WebAPI.Endpoints.Categories;

public class GetCategoriesEndpoint : EndpointWithoutRequest<List<CategoryDto>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetCategoriesEndpoint> _logger;

    public GetCategoriesEndpoint(IMediator mediator, ILogger<GetCategoriesEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/categories");
        AllowAnonymous();
        Description(d => d
            .WithTags("Categories")
            .WithOpenApi()
            .Produces<List<CategoryDto>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var request = new GetCategoriesQuery();
        var result = await _mediator.Send(request, ct);

        if (result.IsSuccess)
        {
            await SendOkAsync(result.Value, ct);
        }
        else
        {
            await SendErrorsAsync(400, ct);
        }
    }
}