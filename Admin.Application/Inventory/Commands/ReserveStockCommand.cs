using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Commands;
public record ReserveStockCommand : IRequest<Result<Guid>>
{
    public Guid OrderId { get; init; }
    public List<OrderItemReservation> Items { get; init; } = new();
}

public record OrderItemReservation
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, Result<Guid>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReserveStockCommandHandler> _logger;

    public ReserveStockCommandHandler(
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReserveStockCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check all items have sufficient stock
            var availability = await _stockRepository.CheckStockAvailabilityAsync(
                request.Items.ToDictionary(x => x.ProductId, x => x.Quantity),
                cancellationToken);

            var unavailableItems = availability
                .Where(x => !x.Value)
                .Select(x => x.Key)
                .ToList();

            if (unavailableItems.Any())
            {
                return Result<Guid>.Failure(new Error(
                    "InsufficientStock",
                    $"Insufficient stock for products: {string.Join(", ", unavailableItems)}"));
            }

            // Create reservations for each item
            foreach (var item in request.Items)
            {
                var stockItem = await _stockRepository.GetByProductIdAsync(item.ProductId, cancellationToken);
                if (stockItem == null)
                    continue;

                stockItem.ReserveStock(item.Quantity, request.OrderId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(request.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for order {OrderId}", request.OrderId);
            return Result<Guid>.Failure(new Error("StockReservation.Failed", "Failed to reserve stock"));
        }
    }
}
