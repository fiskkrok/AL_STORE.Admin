using Admin.Application.Categories.DTOs;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Categories.Queries;
public record GetCategoryQuery(Guid Id) : IRequest<Result<CategoryDto>>;

public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCategoryQueryHandler> _logger;

    public GetCategoryQueryHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ILogger<GetCategoryQueryHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Category not found: {CategoryId}", request.Id);
                return Result<CategoryDto>.Failure(
                    new Error("Category.NotFound", "Category not found"));
            }

            var dto = _mapper.Map<CategoryDto>(category);
            return Result<CategoryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", request.Id);
            return Result<CategoryDto>.Failure(
                new Error("Category.GetFailed", "Failed to retrieve category"));
        }
    }
}

public class GetCategoryQueryValidator : AbstractValidator<GetCategoryQuery>
{
    public GetCategoryQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Category ID is required");
    }
}
