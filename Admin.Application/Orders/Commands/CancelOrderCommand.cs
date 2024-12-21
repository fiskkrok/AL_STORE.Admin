using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Admin.Application.Orders.Commands;
public record CancelOrderCommand : IRequest<Result<Unit>>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<Unit>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<Unit>.Failure(new Error("Order.NotFound", "Order not found"));

            order.Cancel(request.Reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Order.CancelFailed", ex.Message));
        }
    }
}
