using Admin.Application.Categories.Queries;
using Admin.Application.Products.DTOs;
using Admin.WebAPI.Models.Responses;
using FastEndpoints;
using MediatR;

using Microsoft.AspNetCore.Authorization;

namespace Admin.WebAPI.Endpoints.Categories.GetCatefories;

public class GetCategoriesEndpoint : EndpointWithoutRequest<List<CategoryResponse>>
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
            .Produces<List<CategoryResponse>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        List<CategoryResponse> response;
        var request = new GetCategoriesQuery();
        var result = await _mediator.Send(request, ct);
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
