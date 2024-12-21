using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Commands;
public record CreateStockItemCommand : IRequest<Result<Guid>>
{
    public Guid ProductId { get; init; }
    public int InitialStock { get; init; }
    public int LowStockThreshold { get; init; }
    public bool TrackInventory { get; init; } = true;
}

public class CreateStockItemCommandValidator : AbstractValidator<CreateStockItemCommand>
{
    public CreateStockItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0);
    }
}

public class CreateStockItemCommandHandler : IRequestHandler<CreateStockItemCommand, Result<Guid>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStockItemCommandHandler> _logger;

    public CreateStockItemCommandHandler(
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateStockItemCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _stockRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
            if (existing != null)
                return Result<Guid>.Failure(new Error("StockItem.AlreadyExists", "Stock item already exists for this product"));

            var stockItem = new StockItem(
                request.ProductId,
                request.InitialStock,
                request.LowStockThreshold,
                request.TrackInventory);

            await _stockRepository.AddAsync(stockItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(stockItem.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock item for product {ProductId}", request.ProductId);
            return Result<Guid>.Failure(new Error("StockItem.CreateFailed", "Failed to create stock item"));
        }
    }
}
