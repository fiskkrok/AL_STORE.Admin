using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Inventory.Commands;
public record CommitReservationCommand : IRequest<Result<Unit>>
{
    public Guid OrderId { get; init; }
}

public class CommitReservationCommandHandler : IRequestHandler<CommitReservationCommand, Result<Unit>>
{
    private readonly IStockRepository _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommitReservationCommandHandler> _logger;

    public CommitReservationCommandHandler(
        IStockRepository stockRepository,
        IUnitOfWork unitOfWork,
        ILogger<CommitReservationCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(CommitReservationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await _stockRepository.GetReservationAsync(request.OrderId, cancellationToken);
            if (reservation == null)
                return Result<Unit>.Failure(new Error("Reservation.NotFound", "Reservation not found"));

            var stockItem = await _stockRepository.GetByIdAsync(reservation.StockItemId, cancellationToken);
            if (stockItem == null)
                return Result<Unit>.Failure(new Error("StockItem.NotFound", "Stock item not found"));

            stockItem.CommitReservation(request.OrderId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing reservation for order {OrderId}", request.OrderId);
            return Result<Unit>.Failure(new Error("Reservation.CommitFailed", "Failed to commit reservation"));
        }
    }
}
