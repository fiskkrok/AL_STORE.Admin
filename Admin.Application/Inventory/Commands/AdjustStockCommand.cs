using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Commands;
public record AdjustStockCommand : IRequest<Result<Unit>>
{
    public Guid ProductId { get; init; }
    public int Adjustment { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Result<Unit>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AdjustStockCommandHandler> _logger;

    public AdjustStockCommandHandler(
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<AdjustStockCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var stockItem = await _stockRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (stockItem == null)
                return Result<Unit>.Failure(new Error("StockItem.NotFound", "Stock item not found"));

            stockItem.AdjustStock(request.Adjustment, request.Reason, _currentUser.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for product {ProductId}", request.ProductId);
            return Result<Unit>.Failure(new Error("StockItem.AdjustFailed", "Failed to adjust stock"));
        }
    }
}
