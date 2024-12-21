using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Entities;
using MediatR;

namespace Admin.Application.Categories.Commands;
public record UpdateCategoryCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public int? SortOrder { get; init; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<Unit>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (category == null)
                return Result<Unit>.Failure(new Error("Category.NotFound", "Category not found"));

            // Update basic info
            category.Update(
                command.Name,
                command.Description,
                command.MetaTitle,
                command.MetaDescription,
                _currentUser.Id);

            // Update image if changed
            if (command.ImageUrl != category.ImageUrl)
            {
                category.UpdateImage(command.ImageUrl, _currentUser.Id);
            }

            // Update parent if changed
            if (command.ParentCategoryId != category.ParentCategoryId)
            {
                Category? newParent = null;
                if (command.ParentCategoryId.HasValue)
                {
                    newParent = await _categoryRepository.GetByIdAsync(command.ParentCategoryId.Value, cancellationToken);
                    if (newParent == null)
                        return Result<Unit>.Failure(new Error("Category.ParentNotFound", "Parent category not found"));
                }
                category.UpdateParent(newParent, _currentUser.Id);
            }

            // Update sort order if provided
            if (command.SortOrder.HasValue)
            {
                category.UpdateSortOrder(command.SortOrder.Value, _currentUser.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Category.UpdateFailed", ex.Message));
        }
    }
}
