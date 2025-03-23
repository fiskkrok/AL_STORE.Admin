using Admin.Application.Categories.Commands;

using FastEndpoints;

using MediatR;

namespace Admin.WebAPI.Endpoints.Categories;

public class CreateCategoryEndpoint : Endpoint<CreateCategoryCommand, Guid>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreateCategoryEndpoint> _logger;

    public CreateCategoryEndpoint(IMediator mediator, ILogger<CreateCategoryEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/categories");
        Description(d => d
            .WithTags("Categories")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateCategory")
            .WithOpenApi());
        Policies("ProductsCreate", "FullAdminAccess");
    }

    public override async Task HandleAsync(CreateCategoryCommand req, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(req, ct);

            if (result.IsSuccess)
            {
                await SendCreatedAtAsync<GetCategoryEndpoint>(
                    new { id = result.Value },
                    result.Value,
                    generateAbsoluteUrl: true,
                    cancellation: ct);
            }
            else
            {
                await SendErrorsAsync(400, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            await SendErrorsAsync(500, ct);
        }
    }
}
