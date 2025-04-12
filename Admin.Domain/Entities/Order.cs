using Admin.Domain.Common;
using Admin.Domain.Enums;
using Admin.Domain.Events.Order;
using Admin.Domain.ValueObjects;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
public class Order : AuditableEntity
{
    private readonly List<OrderItem> _items = new();
    private OrderStatus _status = OrderStatus.Pending;
    private string? _notes;
    private DateTime? _cancelledAt;
    private string? _cancellationReason;

    private Order() { } // Required by EF Core

    public Order(
        Guid customerId,
        Address shippingAddress,
        Address billingAddress,
        string? notes = null)
    {
        Guard.Against.Null(customerId, nameof(customerId));
        Guard.Against.Null(shippingAddress, nameof(shippingAddress));
        Guard.Against.Null(billingAddress, nameof(billingAddress));

        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        OrderNumber = GenerateOrderNumber();
        _notes = notes;

        AddDomainEvent(new OrderCreatedDomainEvent(this));
    }

    public string OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status => _status;
    public Money Subtotal => CalculateSubtotal();
    public Money ShippingCost { get; private set; } = Money.Zero();
    public Money Tax { get; private set; } = Money.Zero();
    public Money Total => Subtotal.Add(ShippingCost).Add(Tax);
    public Address ShippingAddress { get; private set; }
    public Address BillingAddress { get; private set; }
    public string? Notes => _notes;
    public DateTime? CancelledAt => _cancelledAt;
    public string? CancellationReason => _cancellationReason;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Payment? Payment { get; private set; }
    public ShippingInfo? ShippingInfo { get; private set; }

    public void AddItem(Product product,ProductVariant variant, int quantity, Money unitPrice)
    {
        Guard.Against.Null(product, nameof(product));
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));
        Guard.Against.Null(unitPrice, nameof(unitPrice));

        if (_status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify items of a non-pending order");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(product.Id, variant.Id, quantity, unitPrice);
            _items.Add(orderItem);
        }

        AddDomainEvent(new OrderItemAddedDomainEvent(this, product.Id, quantity));
    }

    public void RemoveItem(Guid productId)
    {
        if (_status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify items of a non-pending order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            AddDomainEvent(new OrderItemRemovedDomainEvent(this, productId));
        }
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        Guard.Against.Null(newAddress, nameof(newAddress));

        if (_status == OrderStatus.Shipped || _status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot update shipping address after order has been shipped");

        ShippingAddress = newAddress;
        AddDomainEvent(new OrderShippingAddressUpdatedDomainEvent(this));
    }

    public void SetShippingInfo(
        string carrier,
        string trackingNumber,
        Money shippingCost,
        DateTime estimatedDeliveryDate)
    {
        if (_status != OrderStatus.Processing && _status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Cannot set shipping info in current state");

        ShippingInfo = new ShippingInfo(
            carrier,
            trackingNumber,
            estimatedDeliveryDate);

        ShippingCost = shippingCost;

        AddDomainEvent(new OrderShippingInfoSetDomainEvent(this));
    }

    public void AddPayment(Payment payment)
    {
        Guard.Against.Null(payment, nameof(payment));

        if (_status != OrderStatus.Pending && _status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Cannot add payment in current state");

        if (payment.Amount != Total)
            throw new InvalidOperationException("Payment amount must match order total");

        Payment = payment;
        UpdateStatus(OrderStatus.Confirmed);

        AddDomainEvent(new OrderPaymentAddedDomainEvent(this));
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!IsValidStatusTransition(_status, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {_status} to {newStatus}");

        var oldStatus = _status;
        _status = newStatus;

        AddDomainEvent(new OrderStatusChangedDomainEvent(this, oldStatus, newStatus));
    }

    public void Cancel(string reason)
    {
        if (_status == OrderStatus.Shipped || _status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel order after shipping");

        _status = OrderStatus.Cancelled;
        _cancelledAt = DateTime.UtcNow;
        _cancellationReason = reason;

        AddDomainEvent(new OrderCancelledDomainEvent(this));
    }

    private Money CalculateSubtotal()
    {
        if (!_items.Any()) return Money.Zero();

        return _items.Select(item => item.Total)
            .Aggregate((a, b) => a.Add(b));
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
    {
        return (current, next) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Processing) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };
    }

    public void AddVariantItem(Product? product, ProductVariant? variant, int quantity, Money unitPrice)
    {
        Guard.Against.Null(product, nameof(product));
        Guard.Against.Null(variant, nameof(variant));
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));
        Guard.Against.Null(unitPrice, nameof(unitPrice));
        if (_status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot modify items of a non-pending order");
        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id && i.VariantId == variant.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(product.Id, variant.Id, quantity, unitPrice);
            _items.Add(orderItem);
        }
        AddDomainEvent(new OrderItemAddedDomainEvent(this, product.Id, quantity));
    }

    public void AddExternalReference(object source, object externalId, object externalOrderNumber)
    {
        throw new NotImplementedException();
    }
}
