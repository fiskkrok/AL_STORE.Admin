// Admin.Application/Products/Commands/UpdateProductImages/UpdateProductImagesCommand.cs
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Commands.UpdateProductImages;

public record UpdateProductImagesCommand : IRequest<Result<Unit>>
{
    public Guid ProductId { get; init; }
    public List<ProductImageUpdateDto> ImageUpdates { get; init; } = new();
}

public record ProductImageUpdateDto
{
    public Guid Id { get; init; }
    public bool? IsPrimary { get; init; }
    public int? SortOrder { get; init; }
    public string? Alt { get; init; }
}

public class UpdateProductImagesCommandHandler : IRequestHandler<UpdateProductImagesCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateProductImagesCommandHandler> _logger;

    public UpdateProductImagesCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateProductImagesCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateProductImagesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdWithImagesAsync(command.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<Unit>.Failure(new Error("Product.NotFound", $"Product with ID {command.ProductId} was not found"));
            }

            // Keep track of which image needs to be set as primary
            Guid? primaryImageId = null;

            // First pass: update all non-primary properties
            foreach (var update in command.ImageUpdates)
            {
                var image = product.Images.FirstOrDefault(i => i.Id == update.Id);
                if (image == null) continue;

                if (update.SortOrder.HasValue)
                {
                    image.UpdateSortOrder(update.SortOrder.Value);
                }

                if (update.Alt != null)
                {
                    image.UpdateAlt(update.Alt);
                }

                if (update.IsPrimary == true)
                {
                    primaryImageId = update.Id;
                }
            }

            // Second pass: handle primary image if needed
            if (primaryImageId.HasValue)
            {
                // Unset primary for all images
                foreach (var image in product.Images)
                {
                    image.SetAsPrimary(false);
                }

                // Set the new primary
                var primaryImage = product.Images.FirstOrDefault(i => i.Id == primaryImageId);
                if (primaryImage != null)
                {
                    primaryImage.SetAsPrimary(true);
                }
            }

            // Set modified by
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product images for product {ProductId}", command.ProductId);
            return Result<Unit>.Failure(new Error("ProductImages.UpdateFailed", "Failed to update product images"));
        }
    }
}