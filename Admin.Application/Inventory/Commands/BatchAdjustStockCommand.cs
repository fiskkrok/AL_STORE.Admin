using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Commands;
public record BatchAdjustStockCommand : IRequest<Result<Unit>>
{
    public List<StockAdjustment> Adjustments { get; init; } = new();
    public string Reason { get; init; } = string.Empty;
}

public record StockAdjustment
{
    public Guid ProductId { get; init; }
    public int Adjustment { get; init; }
}

public class BatchAdjustStockCommandValidator : AbstractValidator<BatchAdjustStockCommand>
{
    public BatchAdjustStockCommandValidator()
    {
        RuleFor(x => x.Adjustments).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class BatchAdjustStockCommandHandler : IRequestHandler<BatchAdjustStockCommand, Result<Unit>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<BatchAdjustStockCommandHandler> _logger;

    public BatchAdjustStockCommandHandler(
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<BatchAdjustStockCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(BatchAdjustStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var adjustment in request.Adjustments)
            {
                var stockItem = await _stockRepository.GetByProductIdAsync(adjustment.ProductId, cancellationToken);
                if (stockItem == null) continue;

                stockItem.AdjustStock(adjustment.Adjustment, request.Reason, _currentUser.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing batch stock adjustment");
            return Result<Unit>.Failure(new Error("BatchAdjustment.Failed", "Failed to perform batch adjustment"));
        }
    }
}
