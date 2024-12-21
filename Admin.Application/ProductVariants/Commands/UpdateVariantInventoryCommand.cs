using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using FluentValidation;

using MediatR;

namespace Admin.Application.ProductVariants.Commands;
public record UpdateVariantInventoryCommand : IRequest<Result<Unit>>
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public int Stock { get; init; }
    public bool TrackInventory { get; init; }
    public bool AllowBackorders { get; init; }
    public int? LowStockThreshold { get; init; }
}
public class UpdateVariantInventoryCommandValidator
    : AbstractValidator<UpdateVariantInventoryCommand>
{
    public UpdateVariantInventoryCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.VariantId)
            .NotEmpty();

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.LowStockThreshold)
            .GreaterThan(0)
            .When(x => x.LowStockThreshold.HasValue);
    }
}
public class UpdateVariantInventoryCommandHandler
    : IRequestHandler<UpdateVariantInventoryCommand, Result<Unit>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateVariantInventoryCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(
        UpdateVariantInventoryCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await _productRepository.GetVariantByIdAsync(
            command.VariantId,
            cancellationToken);

        if (variant == null || variant.ProductId != command.ProductId)
            return Result<Unit>.Failure(
                new Error("Variant.NotFound", "Variant not found"));

        variant.SetInventoryTracking(command.TrackInventory, _currentUser.Id);
        variant.SetBackorderAllowance(command.AllowBackorders, _currentUser.Id);
        variant.SetLowStockThreshold(command.LowStockThreshold, _currentUser.Id);
        variant.UpdateStock(command.Stock, _currentUser.Id);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}