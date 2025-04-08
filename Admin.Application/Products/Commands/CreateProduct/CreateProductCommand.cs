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

            // Create product using mapper
            var product = _mapper.Map<Product>(command);

            // Add product to DbContext
            _dbContext.Products.Add(product);

            // Save changes
            await _dbContext.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Created new product {ProductId}", product.Id);

            return Result<Guid>.Success(product.Id);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            Logger.LogError(ex, "Error creating product");
            return Result<Guid>.Failure(
                new Error("Product.CreateFailed", "Failed to create product due to an unexpected error"));
        }
    }
}
