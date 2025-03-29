// Admin.Application/Products/Commands/DeleteProduct/DeleteProductCommand.cs
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Result<Unit>>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<DeleteProductCommandHandler> logger, ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _dbContext = dbContext;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = _currentUser.Id;
            var product = await _dbContext.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
            {
                return Result<Unit>.Failure(
                    new Error("Product.NotFound", $"Product with ID {request.Id} was not found"));
            }

            product.Delete(user); // This will mark it as inactive and raise the domain event
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", request.Id);
            return Result<Unit>.Failure(
                new Error("Product.DeleteError", "Failed to delete product"));
        }
    }
}