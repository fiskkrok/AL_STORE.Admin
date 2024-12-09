using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Admin.Application.ProductVariants.Commands;
public record CreateProductVariantCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int Stock { get; init; }
    public List<ProductAttributeRequest> Attributes { get; init; } = new();

    public CreateProductVariantCommand(
        Guid productId,
        string sku,
        decimal price,
        string currency,
        int stock,
        List<ProductAttributeRequest> attributes)
    {
        ProductId = productId;
        Sku = sku;
        Price = price;
        Currency = currency;
        Stock = stock;
        Attributes = attributes;
    }
}

public class CreateProductVariantCommandHandler : IRequestHandler<CreateProductVariantCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateProductVariantCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product == null)
                return Result<Guid>.Failure(new Error("Product.NotFound", $"Product with ID {command.ProductId} was not found"));

            var variant = new ProductVariant(
                command.Sku,
                Money.From(command.Price, command.Currency),
                command.Currency,
                command.Stock,
                product.Id);

            foreach (var attr in command.Attributes)
            {
                variant.AddAttribute(
                    ProductAttribute.Create(attr.Name, attr.Value, attr.Type));
            }

            product.AddVariant(variant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(variant.Id);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            // Log the exception here
            return Result<Guid>.Failure(new Error("Variant.CreateFailed", "Failed to create product variant"));
        }
    }
}

public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantCommand>
{
    private readonly IProductRepository _productRepository;

    public CreateProductVariantValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .MustAsync(ProductExists)
            .WithMessage("Product does not exist");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be in ISO 4217 format");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Attributes)
            .SetValidator(new ProductAttributeValidator());
    }

    private async Task<bool> ProductExists(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        return product != null;
    }
}