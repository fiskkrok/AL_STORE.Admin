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
            Category? parentCategory = null;
            if (command.ParentCategoryId.HasValue)
            {
                parentCategory = await _categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
                if (parentCategory == null)
                    return Result<Guid>.Failure(new Error("Category.ParentNotFound", "Parent category not found"));
            }

            // Check if the category with the same ID already exists to avoid duplicate key issues
            var existingCategory = await _categoryRepository.GetByIdAsync(Guid.NewGuid(), cancellationToken);
            if (existingCategory != null)
            {
                return Result<Guid>.Failure(new Error("Category.DuplicateId", "Category with the same ID already exists"));
            }

            var category = new Category(
                command.Name,
                command.Description,
                command.ImageUrl,
                parentCategory);

            if (!string.IsNullOrWhiteSpace(command.MetaTitle) || !string.IsNullOrWhiteSpace(command.MetaDescription))
            {
                category.Update(
                    command.Name,
                    command.Description,
                    command.MetaTitle,
                    command.MetaDescription,
                    _currentUser.Id);
            }

            await _categoryRepository.AddAsync(category, cancellationToken);
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
