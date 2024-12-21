using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.ProductVariants.Commands;
public record DeleteProductVariantCommand(Guid productId,Guid variantId ) : IRequest<Result<Unit>>;

public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductVariantCommandHandler> _logger;

    public DeleteProductVariantCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductVariantCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(
        DeleteProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.productId, cancellationToken);

            if (product == null)
            {
                return Result<Unit>.Failure(
                    new Error("Product.NotFound", $"Product with ID {request.productId} was not found"));
            }

            product.Delete(); // This will mark it as inactive and raise the domain event
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", request.productId);
            return Result<Unit>.Failure(
                new Error("Product.DeleteError", "Failed to delete product"));
        }
    }
}
