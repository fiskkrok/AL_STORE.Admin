using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Admin.Application.ProductVariants.Commands;
public record UpdateVariantPricingCommand : IRequest<Result<Unit>>
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? CompareAtPrice { get; init; }
    public decimal? CostPrice { get; init; }
}

public class UpdateVariantPricingCommandValidator : AbstractValidator<UpdateVariantPricingCommand>
{
    public UpdateVariantPricingCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be in ISO 4217 format");

        RuleFor(x => x.CompareAtPrice)
            .GreaterThan(0)
            .When(x => x.CompareAtPrice.HasValue);

        RuleFor(x => x.CostPrice)
            .GreaterThan(0)
            .When(x => x.CostPrice.HasValue)
            .LessThanOrEqualTo(x => x.Price)
            .When(x => x.CostPrice.HasValue)
            .WithMessage("Cost price must be less than or equal to selling price");
    }
}
internal class UpdateVariantPricingCommandHandler 
    : IRequestHandler<UpdateVariantPricingCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
private readonly IUnitOfWork _unitOfWork;
private readonly ICurrentUser _currentUser;

public UpdateVariantPricingCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
{
    _productRepository = productRepository;
    _unitOfWork = unitOfWork;
    _currentUser = currentUser;
}

public async Task<Result<Unit>> Handle(
    UpdateVariantPricingCommand command,
    CancellationToken cancellationToken)
{
    var variant = await _productRepository.GetVariantByIdAsync(
        command.VariantId,
        cancellationToken);

    if (variant == null || variant.ProductId != command.ProductId)
        return Result<Unit>.Failure(
            new Error("Variant.NotFound", "Variant not found"));

    variant.UpdatePrice(
        Money.From(command.Price, command.Currency),
        _currentUser.Id);

    if (command.CompareAtPrice.HasValue)
    {
        variant.UpdateCompareAtPrice(
            Money.From(command.CompareAtPrice.Value, command.Currency),
            _currentUser.Id);
    }

    if (command.CostPrice.HasValue)
    {
        variant.UpdateCostPrice(
            Money.From(command.CostPrice.Value, command.Currency),
            _currentUser.Id);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result<Unit>.Success(Unit.Value);
}
}
