using Admin.Application.Categories.Queries;
using Admin.WebAPI.Endpoints.Categories.Models;
using Admin.WebAPI.Endpoints.Products.Models;

using FastEndpoints;

using MediatR;
using CategoryResponse = Admin.WebAPI.Endpoints.Products.Models.CategoryResponse;

namespace Admin.WebAPI.Endpoints.Categories;

public class GetCategoriesEndpoint(IMediator mediator, ILogger<GetCategoriesEndpoint> logger)
    : EndpointWithoutRequest<List<CategoryResponse>>
{
    private readonly ILogger<GetCategoriesEndpoint> _logger = logger;
    public override void Configure()
    {
        Get("/categories");
        AllowAnonymous();
        Description(d => d
            .WithTags("Categories")
            .WithOpenApi()
            .Produces<List<CategoryResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<CategoryResponse> response;
        var request = new GetCategoriesQuery();
        var result = await mediator.Send(request, ct);
        if (result.IsSuccess)
        {
          response =  result.Value.Select(CategoryResponse.FromDto).ToList();



            await SendOkAsync(response, ct);
        }
        else
        {
            await SendErrorsAsync(400, ct);
        }
    }

}
