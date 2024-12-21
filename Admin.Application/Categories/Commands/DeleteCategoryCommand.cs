using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using MediatR;

namespace Admin.Application.Categories.Commands;
public record DeleteCategoryCommand(Guid Id) : IRequest<Result<Unit>>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<Unit>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
            if (category == null)
                return Result<Unit>.Failure(new Error("Category.NotFound", "Category not found"));

            category.Delete(_currentUser.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Category.DeleteFailed", ex.Message));
        }
    }
}
