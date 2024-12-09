using Admin.Domain.Common;
using Admin.Domain.Events;
using Admin.Domain.ValueObjects;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
public class ProductVariant : AuditableEntity
{
    private readonly List<ProductAttribute> _attributes = new();
    private string _sku = string.Empty;
    private decimal _price;
    private string _currency = "USD";
    private int _stock;

    // Private constructor for EF Core
    private ProductVariant()
    {
    }

    public ProductVariant(
        string sku,
        decimal price,
        string currency,
        int stock,
        Guid productId)
    {
        Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
        Guard.Against.NegativeOrZero(price, nameof(price));
        Guard.Against.Negative(stock, nameof(stock));
        Guard.Against.NullOrWhiteSpace(currency, nameof(currency));
        Guard.Against.InvalidInput(currency, nameof(currency), c => c.Length == 3);

        _sku = sku;
        _price = price;
        _currency = currency;
        _stock = stock;
        ProductId = productId;
    }

    public string Sku => _sku;
    public Money Price => Money.From(_price, _currency);
    public int Stock => _stock;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    public void UpdateSku(string sku, string? modifiedBy = null)
    {
        Guard.Against.NullOrWhiteSpace(sku, nameof(sku));

        if (_sku == sku) return;

        _sku = sku;
        SetModified(modifiedBy);
    }

    public void UpdatePrice(Money price, string? modifiedBy = null)
    {
        Guard.Against.Null(price, nameof(price));

        if (_price == price.Amount && _currency == price.Currency) return;

        _price = price.Amount;
        _currency = price.Currency;
        SetModified(modifiedBy);
    }

    public void UpdateStock(int stock, string? modifiedBy = null)
    {
        Guard.Against.Negative(stock, nameof(stock));

        if (_stock == stock) return;

        var oldStock = _stock;
        _stock = stock;
        SetModified(modifiedBy);

        AddDomainEvent(new ProductVariantStockUpdatedDomainEvent(this.Product,this, oldStock, stock));
    }

    public void AddAttribute(ProductAttribute attribute)
    {
        Guard.Against.Null(attribute, nameof(attribute));
        _attributes.Add(attribute);
    }

    public void UpdateAttributes(List<ProductAttribute> attributes, string? modifiedBy = null)
    {
        Guard.Against.Null(attributes, nameof(attributes));

        _attributes.Clear();
        _attributes.AddRange(attributes);
        SetModified(modifiedBy);
    }
}
