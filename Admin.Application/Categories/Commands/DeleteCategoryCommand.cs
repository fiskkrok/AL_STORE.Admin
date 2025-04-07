using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Admin.Application.Categories.Commands;
public record DeleteCategoryCommand(Guid Id) : IRequest<Result<Unit>>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDomainEventService _domainEventService;

    public DeleteCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDomainEventService domainEventService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _domainEventService = domainEventService;
    }

    public async Task<Result<Unit>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Load the category with its relationships directly from DbContext
            var category = await _dbContext.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

            if (category == null)
                return Result<Unit>.Failure(new Error("Category.NotFound", "Category not found"));

            // Check if category has products
            if (category.Products.Any())
                return Result<Unit>.Failure(new Error("Category.HasProducts",
                    "Cannot delete category with associated products"));

            // Call the domain entity's Delete method, which will mark it as inactive
            // and add the appropriate domain event
            category.Delete(_currentUser.Id);

            // Also handle subcategories
            foreach (var subCategory in category.SubCategories.ToList())
            {
                subCategory.Delete(_currentUser.Id);
            }

            // Save changes to persist the updated IsActive status
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Category.DeleteFailed", ex.Message));
        }
    }
}
