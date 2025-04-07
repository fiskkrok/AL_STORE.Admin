using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Entities;
using MediatR;

namespace Admin.Application.Categories.Commands;
public record CreateCategoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public Guid? ParentCategoryId { get; init; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICacheService _cacheService;

    private const string CategoriesListKey = "categories:list:dto";

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ICacheService cacheService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // First, just check if the parent exists (without loading the actual entity)
            if (command.ParentCategoryId.HasValue)
            {
                bool parentExists = await _categoryRepository.ExistsAsync(
                    command.ParentCategoryId.Value, cancellationToken);

                if (!parentExists)
                    return Result<Guid>.Failure(new Error("Category.ParentNotFound", "Parent category not found"));
            }

            // Create the category with just the name, description, and imageUrl
            var category = new Category(
                command.Name,
                command.Description,
                command.ImageUrl);

            // Set metadata if provided
            if (!string.IsNullOrWhiteSpace(command.MetaTitle) || !string.IsNullOrWhiteSpace(command.MetaDescription))
            {
                category.Update(
                    command.Name,
                    command.Description,
                    command.MetaTitle,
                    command.MetaDescription,
                    _currentUser.Id);
            }

            // Add the category to the repository first
            await _categoryRepository.AddAsync(category, cancellationToken);

            // Then establish the parent relationship AFTER the entity is added and tracked
            if (command.ParentCategoryId.HasValue)
            {
                // After the entity is tracked, load the parent and set it
                var parentCategory = await _categoryRepository.GetByIdAsync(
                    command.ParentCategoryId.Value, cancellationToken);

                if (parentCategory != null)
                {
                    category.UpdateParent(parentCategory, _currentUser.Id);
                }
            }

            // Now save everything
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cacheService.RemoveByPrefixAsync(CategoriesListKey, cancellationToken);

            return Result<Guid>.Success(category.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(new Error("Category.CreateFailed", ex.Message));
        }
    }
}
