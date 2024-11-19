using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Admin.Application.Common.Models;

namespace Admin.Application.Products.Commands.UpdateProduct;
public record UpdateProductCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public Guid CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public List<FileUploadRequest> NewImages { get; init; } = new();
    public List<Guid> ImageIdsToRemove { get; init; } = new();
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
            if (product == null)
                return Result<Unit>.Failure(new Error("Product.NotFound", $"Product with ID {command.Id} was not found"));

            var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
            if (category == null)
                return Result<Unit>.Failure(new Error("Category.NotFound", $"Category with ID {command.CategoryId} was not found"));

            Category? subCategory = null;
            if (command.SubCategoryId.HasValue)
            {
                subCategory = await _categoryRepository.GetByIdAsync(command.SubCategoryId.Value, cancellationToken);
                if (subCategory == null)
                    return Result<Unit>.Failure(new Error("SubCategory.NotFound", $"SubCategory with ID {command.SubCategoryId} was not found"));
            }

            // Update core product details
            product.Update(
                command.Name,
                command.Description,
                Money.From(command.Price, command.Currency),
                category,
                subCategory,
                _currentUser.Id);

            // Handle image removals
            foreach (var imageId in command.ImageIdsToRemove)
            {
                var image = product.Images.FirstOrDefault(i => i.Id == imageId);
                if (image != null)
                {
                    await _fileStorage.DeleteAsync(image.Url, cancellationToken);
                    product.RemoveImage(image, _currentUser.Id);
                }
            }

            // Handle new images
            foreach (var imageFile in command.NewImages)
            {
                var uploadResult = await _fileStorage.UploadAsync(imageFile, cancellationToken);
                product.AddImage(uploadResult.Url, uploadResult.FileName, uploadResult.Size, _currentUser.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            // Log the exception here
            return Result<Unit>.Failure(new Error("Product.UpdateFailed", "Failed to update product due to an unexpected error"));
        }
    }
}
