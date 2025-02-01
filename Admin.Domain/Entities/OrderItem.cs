using Admin.Domain.Common;
using Admin.Domain.ValueObjects;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
public class OrderItem : AuditableEntity
{
    private int _quantity;
    private Money _unitPrice;

    private OrderItem() { } // Required by EF Core

    public OrderItem(Guid productId, Guid variantId, int quantity, Money unitPrice)
    {
        Guard.Against.NegativeOrZero(quantity, nameof(quantity));
        Guard.Against.Null(unitPrice, nameof(unitPrice));
        ProductId = productId;
        VariantId = variantId;
        _quantity = quantity;
        _unitPrice = unitPrice;
    }

    public Guid ProductId { get; private set; }

    public int Quantity => _quantity;
    public Money UnitPrice => _unitPrice;
    public Money Total => Money.From(_unitPrice.Amount * _quantity, _unitPrice.Currency);
    public Guid VariantId { get; set; }

    public void UpdateQuantity(int newQuantity)
    {
        Guard.Against.NegativeOrZero(newQuantity, nameof(newQuantity));
        _quantity = newQuantity;
    }
}

