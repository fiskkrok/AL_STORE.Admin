using System.Text.Json;
using Admin.Application.Common.CQRS;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Admin.Application.Products.Commands.CreateProduct;

// Command definition
public record CreateProductCommand : ICommand<Guid>
{
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
    public string Status { get; init; } = "Draft";
    public string Visibility { get; init; } = "Hidden";
    public Guid CategoryId { get; init; }
    public Guid? SubCategoryId { get; init; }
    public List<ProductImageDto> Images { get; init; } = [];
    public List<ProductVariantDto> Variants { get; init; } = [];
    public List<ProductAttributeDto> Attributes { get; init; } = [];
    public ProductSeoDto? Seo { get; init; }
    public ProductDimensionsDto? Dimensions { get; init; }
    public string ProductTypeId { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = [];
}

// Command handler
public class CreateProductCommandHandler : CommandHandler<CreateProductCommand, Guid>
{
    private readonly ICurrentUser _currentUser;
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CreateProductCommandHandler> logger,
        ICurrentUser currentUser,
        IMapper mapper) : base(dbContext, logger)
    {
        _currentUser = currentUser;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public override async Task<Result<Guid>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the product type to validate against
            var productType = await _dbContext.ProductTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(pt => pt.Id == new Guid(command.ProductTypeId), cancellationToken);

            if (productType == null)
            {
                return Result<Guid>.Failure(
                    new Error("ProductType.NotFound", $"Product type {command.ProductTypeId} not found"));
            }

            var typeAttributes = JsonSerializer.Deserialize<List<ProductTypeAttributeDto>>(productType.AttributesJson);

            // Validate required attributes
            if (typeAttributes != null)
            {
                foreach (var requiredAttr in typeAttributes.Where(a => a.IsRequired))
                {
                    if (!command.Attributes.Any(a => a.Name == requiredAttr.Id))
                    {
                        return Result<Guid>.Failure(
                            new Error("Product.MissingRequiredAttribute",
                                $"Required attribute {requiredAttr.Name} is missing"));
                    }
                }
            }
            // Validate category exists
            var category = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == command.CategoryId, cancellationToken);

            if (category == null)
                return Result<Guid>.Failure(
                    new Error("Category.NotFound", $"Category with ID {command.CategoryId} was not found"));

            // Validate subcategory if provided
            if (command.SubCategoryId.HasValue)
            {
                var subCategory = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Id == command.SubCategoryId.Value, cancellationToken);

                if (subCategory == null)
                    return Result<Guid>.Failure(
                        new Error("SubCategory.NotFound", $"SubCategory with ID {command.SubCategoryId} was not found"));
            }

            // Set up mapping context with current user
            var mappingContext = new Dictionary<string, object>
            {
                { "CurrentUser", _currentUser.Id ?? "system" }
            };

            // Create product using mapper with context
            var product = _mapper.Map<Product>(command, opts =>
                opts.Items["CurrentUser"] = _currentUser.Id ?? "system");
            // Manually handle Seo if it exists in the command
            if (command.Seo != null)
            {
                var productSeo = ProductSeo.Create(
                    command.Seo.Title,
                    command.Seo.Description,
                    command.Seo.Keywords ?? new List<string>()
                );
                product.UpdateSeo(productSeo, _currentUser.Id);
            }
            // Add product to DbContext
            _dbContext.Products.Add(product);

            // Save changes
            await _dbContext.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Created new product {ProductId}", product.Id);

            return Result<Guid>.Success(product.Id);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            Logger.LogError(ex, "Error creating product: {ErrorMessage}", ex.Message);
            return Result<Guid>.Failure(
                new Error("Product.CreateFailed", "Failed to create product due to an unexpected error"));
        }
    }
}
