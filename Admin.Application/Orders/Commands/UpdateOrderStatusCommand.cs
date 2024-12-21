using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Admin.Application.Orders.Commands;
public record UpdateOrderStatusCommand : IRequest<Result<Unit>>
{
    public Guid OrderId { get; init; }
    public OrderStatus NewStatus { get; init; }
}

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<Unit>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<Unit>.Failure(new Error("Order.NotFound", "Order not found"));

            order.UpdateStatus(request.NewStatus);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Order.UpdateStatusFailed", ex.Message));
        }
    }
}
