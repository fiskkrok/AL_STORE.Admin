using Admin.Application.Common.CQRS;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;
using Admin.Domain.Enums;
using Admin.Domain.ValueObjects;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Commands.UpdateProduct;
public record UpdateProductCommand : ICommand<Unit>
{
    public Guid Id { get; init; }
    public string ProductTypeId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? CompareAtPrice { get; init; }
    public int Stock { get; init; }
    public int? LowStockThreshold { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Visibility { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public List<ProductImageDto> NewImages { get; init; } = new();
    public List<Guid> ImageIdsToRemove { get; init; } = new();
    public List<ProductVariantDto> NewVariants { get; init; } = new();
    public List<Guid> VariantIdsToRemove { get; init; } = new();
    public List<ProductAttributeDto> Attributes { get; init; } = new();
    public ProductSeoDto? Seo { get; init; }
    public ProductDimensionsDto? Dimensions { get; init; }
    public List<string> Tags { get; init; } = new();
}

public class UpdateProductCommandHandler : CommandHandler<UpdateProductCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorage _fileStorage;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCommandHandler(
        IApplicationDbContext dbContext,
        IFileStorage fileStorage,
        ICurrentUser currentUser,
        ILogger<UpdateProductCommandHandler> logger)
        : base(dbContext, logger)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public override async Task<Result<Unit>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Get the product directly using DbContext
            var product = await _dbContext.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            if (product == null)
            {
                return Result<Unit>.Failure(
                    new Error("Product.NotFound", $"Product with ID {command.Id} was not found"));
            }

            // Get the category
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);

            if (category == null)
            {
                return Result<Unit>.Failure(new Error("Category.NotFound",
                    $"Category with ID {command.CategoryId} was not found"));
            }

            // Get the subcategory if specified
            Category? subCategory = null;
            if (command.SubCategoryId.HasValue)
            {
                subCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == command.SubCategoryId.Value, cancellationToken);
                if (subCategory == null)
                {
                    return Result<Unit>.Failure(new Error("SubCategory.NotFound", $"SubCategory with ID {command.SubCategoryId} was not found"));
                }
            }

            // Get the product type if specified
            ProductType? productType = null;
            if (!string.IsNullOrEmpty(command.ProductTypeId))
            {
                productType =
                    await _dbContext.ProductTypes.FirstOrDefaultAsync(pt => pt.Id == Guid.Parse(command.ProductTypeId),
                        cancellationToken);
                if (productType == null)
                {
                    return Result<Unit>.Failure(new Error("ProductType.NotFound",
                        $"Product type with ID {command.ProductTypeId} was not found"));

                }
            }

            var currentUserId = _currentUser.Id;

            // Update basic properties
            product.Update(
                command.Name,
                command.Description,
                Money.From(command.Price, command.Currency),
                category,
                subCategory,
                currentUserId);

            // Update other properties if they are provided in the command
            if (!string.IsNullOrWhiteSpace(command.ShortDescription))
                product.UpdateShortDescription(command.ShortDescription, currentUserId);

            if (!string.IsNullOrWhiteSpace(command.Barcode))
                product.UpdateBarcode(command.Barcode, currentUserId);

            if (command.CompareAtPrice.HasValue)
                product.UpdateCompareAtPrice(Money.From(command.CompareAtPrice.Value, command.Currency), currentUserId);
            else if (command.CompareAtPrice == null && product.CompareAtPrice != null)
                product.UpdateCompareAtPrice(null, currentUserId);

            if (command.Stock >= 0)
                product.UpdateStock(command.Stock, currentUserId);

            if (command.LowStockThreshold.HasValue)
                product.UpdateLowStockThreshold(command.LowStockThreshold, currentUserId);
            else if (command.LowStockThreshold == null && product.LowStockThreshold != null)
                product.UpdateLowStockThreshold(null, currentUserId);

            if (!string.IsNullOrEmpty(command.Status))
                product.UpdateStatus(Enum.Parse<ProductStatus>(command.Status, true), currentUserId);

            if (!string.IsNullOrEmpty(command.Visibility))
                product.UpdateVisibility(Enum.Parse<ProductVisibility>(command.Visibility, true), currentUserId);

            if (productType != null)
                product.SetProductType(productType, currentUserId);

            // Update SEO information
            if (command.Seo != null)
            {
                var seo = ProductSeo.Create(
                    command.Seo.Title,
                    command.Seo.Description,
                    command.Seo.Keywords);
                product.UpdateSeo(seo, currentUserId);
            }

            // Update dimensions
            if (command.Dimensions != null)
            {
                var dimensions = ProductDimensions.Create(
                    command.Dimensions.Weight,
                    command.Dimensions.Width,
                    command.Dimensions.Height,
                    command.Dimensions.Length,
                    command.Dimensions.Unit);
                product.UpdateDimensions(dimensions, currentUserId);
            }

            // Handle image removals
            if (command.ImageIdsToRemove.Count != 0)
            {
                foreach (var image in command.ImageIdsToRemove.Select(imageId => product.Images.FirstOrDefault(i => i.Id == imageId)).OfType<ProductImage>())
                {
                    product.RemoveImage(image, currentUserId);

                    // Optionally delete the file from storage
                    try
                    {
                        await _fileStorage.DeleteAsync(image.Url, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Log but don't fail the operation
                        Logger.LogWarning(ex, "Failed to delete image file at {Url} for product {ProductId}", image.Url, product.Id);
                    }
                }
            }

            // Add new images
            if (command.NewImages.Count != 0)
            {
                foreach (var imageDto in command.NewImages)
                {
                    product.AddImage(imageDto.Url, imageDto.FileName, imageDto.Size, currentUserId);
                }
            }

            // Handle variant removals
            if (command.VariantIdsToRemove.Count != 0)
            {
                // This should be part of a domain method to properly handle variant removal
                // For now, we'll try to simulate it
                foreach (var variantId in command.VariantIdsToRemove)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
                    if (variant != null)
                    {
                        product.RemoveVariant(variant, currentUserId);


                    }
                }
            }

            // Add new variants
            if (command.NewVariants.Count != 0)
            {
                foreach (var variantDto in command.NewVariants)
                {
                    var variant = new ProductVariant(
                        variantDto.Sku,
                        variantDto.Price,
                        variantDto.Currency,
                        variantDto.Stock,
                        product.Id,
                        variantDto.LowStockThreshold,
                        variantDto.CompareAtPrice,
                        variantDto.CostPrice,
                        variantDto.Barcode);

                    // Add attributes for the variant
                    if (variantDto.Attributes != null && variantDto.Attributes.Any())
                    {
                        foreach (var attrDto in variantDto.Attributes)
                        {
                            var attribute = ProductAttribute.Create(attrDto.Name, attrDto.Value, attrDto.Type);
                            variant.AddAttribute(attribute);
                        }
                    }

                    product.AddVariant(variant);
                }
            }

            // Update attributes
            if (command.Attributes.Count != 0)
            {
                // Remove existing attributes and add new ones
                product.ClearAttributes();

                foreach (var attribute in command.Attributes.Select(attrDto => ProductAttribute.Create(attrDto.Name, attrDto.Value, attrDto.Type)))
                {
                    product.AddAttribute(attribute, currentUserId);
                }
            }

            // Update tags
            if (command.Tags.Count > 0)
            {
                // Get existing tags
                var existingTags = product.Tags.ToList();

                // Remove tags that are no longer present
                foreach (var existingTag in existingTags.Where(existingTag => !command.Tags.Contains(existingTag)))
                {
                    product.RemoveTag(existingTag, currentUserId);
                }

                // Add new tags
                foreach (var newTag in command.Tags.Where(newTag => !existingTags.Contains(newTag)))
                {
                    product.AddTag(newTag, currentUserId);
                }
            }

            // Save the updated product
            await Context.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating product {ProductId}", command.Id);
            return Result<Unit>.Failure(new Error("Product.UpdateFailed", "Failed to update product due to an unexpected error"));
        }
    }
}
