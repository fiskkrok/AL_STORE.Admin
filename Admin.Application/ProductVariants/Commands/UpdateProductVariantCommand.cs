using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;
using MediatR;

namespace Admin.Application.ProductVariants.Commands;
public record UpdateProductVariantCommand : IRequest<Result<Unit>>
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int Stock { get; init; }
    public List<ProductAttributeDto> Attributes { get; init; } = new();
}

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateProductVariantCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateProductVariantCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product == null)
                return Result<Unit>.Failure(new Error("Product.NotFound", $"Product with ID {command.ProductId} was not found"));

            var variant = product.Variants.FirstOrDefault(v => v.Id == command.VariantId);
            if (variant == null)
                return Result<Unit>.Failure(new Error("Variant.NotFound", $"Variant with ID {command.VariantId} was not found"));

            variant.UpdateSku(command.Sku, _currentUser.Id);
            variant.UpdatePrice(Money.From(command.Price, command.Currency), _currentUser.Id);
            variant.UpdateStock(command.Stock, _currentUser.Id);
            variant.UpdateAttributes(command.Attributes.Select(a => ProductAttribute.Create(a.Name, a.Value, a.Type)).ToList());

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            // Log the exception here
            return Result<Unit>.Failure(new Error("Variant.UpdateFailed", "Failed to update product variant"));
        }
    }
}
