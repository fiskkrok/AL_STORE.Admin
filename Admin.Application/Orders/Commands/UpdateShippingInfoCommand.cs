using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Admin.Application.Orders.Commands;
public record UpdateShippingInfoCommand : IRequest<Result<Unit>>
{
    public Guid OrderId { get; init; }
    public string Carrier { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public decimal ShippingCost { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime EstimatedDeliveryDate { get; init; }
}

public class UpdateShippingInfoCommandValidator : AbstractValidator<UpdateShippingInfoCommand>
{
    public UpdateShippingInfoCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Carrier).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).Length(3);
        RuleFor(x => x.EstimatedDeliveryDate).NotEmpty().GreaterThan(DateTime.UtcNow);
    }
}

public class UpdateShippingInfoCommandHandler : IRequestHandler<UpdateShippingInfoCommand, Result<Unit>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShippingInfoCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(UpdateShippingInfoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<Unit>.Failure(new Error("Order.NotFound", "Order not found"));

            order.SetShippingInfo(
                request.Carrier,
                request.TrackingNumber,
                Money.From(request.ShippingCost, request.Currency),
                request.EstimatedDeliveryDate);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Order.UpdateShippingInfoFailed", ex.Message));
        }
    }
}

