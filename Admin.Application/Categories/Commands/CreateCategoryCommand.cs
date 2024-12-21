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

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
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

            return Result<Guid>.Success(category.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(new Error("Category.CreateFailed", ex.Message));
        }
    }
}
