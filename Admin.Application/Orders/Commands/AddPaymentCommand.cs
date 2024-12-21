using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.Enums;
using Admin.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Admin.Application.Orders.Commands;
public record AddPaymentCommand : IRequest<Result<Unit>>
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public PaymentMethod Method { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
}

public class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.Method).IsInEnum();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).Length(3);
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand, Result<Unit>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPaymentCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<Unit>.Failure(new Error("Order.NotFound", "Order not found"));

            var payment = new Payment(
                request.TransactionId,
                request.Method,
                Money.From(request.Amount, request.Currency),
                request.Status,
                DateTime.UtcNow);

            order.AddPayment(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure(new Error("Order.AddPaymentFailed", ex.Message));
        }
    }
}