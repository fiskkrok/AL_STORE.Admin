using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.ProductVariants.Commands;
public record BatchUpdateVariantsCommand : IRequest<Result<List<Guid>>>
{
    public List<UpdateVariantRequest> Variants { get; init; } = new();
}

public record UpdateVariantRequest
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string? Sku { get; init; }
    public decimal? Price { get; init; }
    public int? Stock { get; init; }
    public List<ProductAttributeRequest>? Attributes { get; init; }
}

public class BatchUpdateVariantsCommandHandler
    : IRequestHandler<BatchUpdateVariantsCommand, Result<List<Guid>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<BatchUpdateVariantsCommandHandler> _logger;

    public BatchUpdateVariantsCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<BatchUpdateVariantsCommandHandler> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<List<Guid>>> Handle(
        BatchUpdateVariantsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedVariants = new List<Guid>();
            var errors = new List<string>();

            // Group variants by product to minimize database calls
            var variantsByProduct = request.Variants
                .GroupBy(v => v.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var productGroup in variantsByProduct)
            {
                var product = await _productRepository.GetByIdAsync(
                    productGroup.Key, cancellationToken);

                if (product == null)
                {
                    errors.Add($"Product {productGroup.Key} not found");
                    continue;
                }

                foreach (var variantUpdate in productGroup.Value)
                {
                    var variant = product.Variants
                        .FirstOrDefault(v => v.Id == variantUpdate.VariantId);

                    if (variant == null)
                    {
                        errors.Add($"Variant {variantUpdate.VariantId} not found");
                        continue;
                    }

                    try
                    {
                        UpdateVariant(variant, variantUpdate);
                        updatedVariants.Add(variant.Id);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error updating variant {variant.Id}: {ex.Message}");
                    }
                }
            }

            if (errors.Any())
                return Result<List<Guid>>.Failure(
                    new Error("BatchUpdate.PartialFailure",
                        string.Join(Environment.NewLine, errors)));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<List<Guid>>.Success(updatedVariants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch update variants");
            return Result<List<Guid>>.Failure(
                new Error("BatchUpdate.Failed", "Failed to update variants"));
        }
    }

    private void UpdateVariant(
        ProductVariant variant,
        UpdateVariantRequest update)
    {
        if (update.Sku != null)
            variant.UpdateSku(update.Sku, _currentUser.Id);

        if (update.Price.HasValue)
            variant.UpdatePrice(
                Money.From(update.Price.Value, variant.Price.Currency),
                _currentUser.Id);

        if (update.Stock.HasValue)
            variant.UpdateStock(update.Stock.Value, _currentUser.Id);

        if (update.Attributes != null)
            variant.UpdateAttributes(
                update.Attributes.Select(a =>
                    ProductAttribute.Create(a.Name, a.Value, a.Type))
                .ToList(),
                _currentUser.Id);
    }
}