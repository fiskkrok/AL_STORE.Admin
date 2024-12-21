using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Orders.DTOs;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;
using FluentValidation;

using MediatR;

namespace Admin.Application.Orders.Commands;
public record CreateOrderCommand : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; init; }
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public List<CreateOrderItemCommand> Items { get; init; } = new();
    public string? Notes { get; init; }
}

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.ShippingAddress).NotNull();
        RuleFor(x => x.BillingAddress).NotNull();
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create shipping and billing addresses
            var shippingAddress = new Address(
                request.ShippingAddress.Street,
                request.ShippingAddress.City,
                request.ShippingAddress.State,
            request.ShippingAddress.Country,
                request.ShippingAddress.PostalCode);

            var billingAddress = new Address(
                request.BillingAddress.Street,
                request.BillingAddress.City,
                request.BillingAddress.State,
                request.BillingAddress.Country,
                request.BillingAddress.PostalCode);

            // Create order
            var order = new Order(
                request.CustomerId,
                shippingAddress,
                billingAddress,
                request.Notes);

            // Add items
            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                    return Result<Guid>.Failure(new Error("Order.ProductNotFound", $"Product {item.ProductId} not found"));

                order.AddItem(product, item.Quantity, product.Price);
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(order.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(new Error("Order.CreateFailed", ex.Message));
        }
    }
}
