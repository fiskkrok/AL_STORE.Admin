using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Commands;
public record UpdateStockSettingsCommand : IRequest<Result<Unit>>
{
    public Guid ProductId { get; init; }
    public int? LowStockThreshold { get; init; }
    public bool? TrackInventory { get; init; }
}

public class UpdateStockSettingsCommandValidator : AbstractValidator<UpdateStockSettingsCommand>
{
    public UpdateStockSettingsCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.LowStockThreshold)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LowStockThreshold.HasValue);
    }
}

public class UpdateStockSettingsCommandHandler : IRequestHandler<UpdateStockSettingsCommand, Result<Unit>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UpdateStockSettingsCommandHandler> _logger;

    public UpdateStockSettingsCommandHandler(
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateStockSettingsCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateStockSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var stockItem = await _stockRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (stockItem == null)
                return Result<Unit>.Failure(new Error("StockItem.NotFound", "Stock item not found"));

            if (request.LowStockThreshold.HasValue)
                stockItem.UpdateLowStockThreshold(request.LowStockThreshold.Value, _currentUser.Id);

            if (request.TrackInventory.HasValue)
                stockItem.SetTrackInventory(request.TrackInventory.Value, _currentUser.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock settings for product {ProductId}", request.ProductId);
            return Result<Unit>.Failure(new Error("StockItem.UpdateSettingsFailed", "Failed to update stock settings"));
        }
    }
}
