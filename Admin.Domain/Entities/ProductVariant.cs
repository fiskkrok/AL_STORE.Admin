using System.Text.Json.Serialization;

using Admin.Domain.Common;
using Admin.Domain.Common.Exceptions;
using Admin.Domain.Events.ProductVariant;
using Admin.Domain.ValueObjects;

using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
public class ProductVariant : AuditableEntity
{
    private readonly List<ProductAttribute> _attributes = new();
    private readonly List<ProductImage> _images = new();
    private string _sku = string.Empty;
    private decimal _price;
    private string _currency = "USD";
    private int _stock;
    private int? _lowStockThreshold;
    private decimal? _compareAtPrice;
    private decimal? _costPrice;
    private string? _barcode;
    private bool _trackInventory = true;
    private bool _allowBackorders;
    private int _sortOrder;

    private ProductVariant() { } // For EF Core

    public ProductVariant(
        string sku,
        decimal price,
        string currency,
        int stock,
        Guid productId,
        int? lowStockThreshold = null,
        decimal? compareAtPrice = null,
        decimal? costPrice = null,
        string? barcode = null)
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
        _lowStockThreshold = lowStockThreshold;
        _compareAtPrice = compareAtPrice;
        _costPrice = costPrice;
        _barcode = barcode;
        ProductId = productId;
    }

    // Properties
    public string Sku => _sku;
    public Money Price => Money.From(_price, _currency);
    public Money? CompareAtPrice => _compareAtPrice.HasValue ? Money.From(_compareAtPrice.Value, _currency) : null;
    public Money? CostPrice => _costPrice.HasValue ? Money.From(_costPrice.Value, _currency) : null;
    public int Stock => _stock;
    public string? Barcode => _barcode;
    public bool TrackInventory => _trackInventory;
    public bool AllowBackorders => _allowBackorders;
    public int? LowStockThreshold => _lowStockThreshold;
    public int SortOrder => _sortOrder;
    public Guid ProductId { get; private set; }
    [JsonIgnore]
    public Product Product { get; private set; } = null!;
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    // Rich behavior methods
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

        var oldPrice = Money.From(_price, _currency);
        _price = price.Amount;
        _currency = price.Currency;
        SetModified(modifiedBy);

        AddDomainEvent(new VariantPriceUpdatedDomainEvent(this, oldPrice, price));
    }

    public void UpdateCompareAtPrice(Money? compareAtPrice, string? modifiedBy = null)
    {
        if (compareAtPrice?.Currency != _currency)
            throw new DomainException($"Compare at price currency must match variant currency: {_currency}");

        _compareAtPrice = compareAtPrice?.Amount;
        SetModified(modifiedBy);
    }

    public void UpdateCostPrice(Money? costPrice, string? modifiedBy = null)
    {
        if (costPrice?.Currency != _currency)
            throw new DomainException($"Cost price currency must match variant currency: {_currency}");

        _costPrice = costPrice?.Amount;
        SetModified(modifiedBy);
    }

    public void UpdateStock(int stock, string? modifiedBy = null)
    {
        Guard.Against.Negative(stock, nameof(stock));

        if (!_trackInventory || _stock == stock) return;

        var oldStock = _stock;
        _stock = stock;
        SetModified(modifiedBy);

        AddDomainEvent(new ProductVariantStockUpdatedDomainEvent(this.Product, this, oldStock, stock));

        // Check low stock threshold
        if (_lowStockThreshold.HasValue && stock <= _lowStockThreshold.Value)
        {
            AddDomainEvent(new VariantLowStockDomainEvent(this));
        }
    }

    public void SetInventoryTracking(bool trackInventory, string? modifiedBy = null)
    {
        if (_trackInventory == trackInventory) return;

        _trackInventory = trackInventory;
        SetModified(modifiedBy);
    }

    public void SetBackorderAllowance(bool allowBackorders, string? modifiedBy = null)
    {
        if (_allowBackorders == allowBackorders) return;

        _allowBackorders = allowBackorders;
        SetModified(modifiedBy);
    }

    public void SetLowStockThreshold(int? threshold, string? modifiedBy = null)
    {
        if (threshold.HasValue)
            Guard.Against.NegativeOrZero(threshold.Value, nameof(threshold));

        if (_lowStockThreshold == threshold) return;

        _lowStockThreshold = threshold;
        SetModified(modifiedBy);

        // Check if we're now in low stock state
        if (threshold.HasValue && _stock <= threshold.Value)
        {
            AddDomainEvent(new VariantLowStockDomainEvent(this));
        }
    }

    public void UpdateBarcode(string? barcode, string? modifiedBy = null)
    {
        if (_barcode == barcode) return;

        _barcode = barcode;
        SetModified(modifiedBy);
    }

    public void AddAttribute(ProductAttribute attribute)
    {
        Guard.Against.Null(attribute, nameof(attribute));

        if (_attributes.Any(a => a.Name == attribute.Name))
            throw new DomainException($"Attribute with name '{attribute.Name}' already exists");

        _attributes.Add(attribute);
    }

    public void UpdateAttributes(List<ProductAttribute> attributes, string? modifiedBy = null)
    {
        Guard.Against.Null(attributes, nameof(attributes));

        // Check for duplicate attribute names
        var duplicateNames = attributes
            .GroupBy(a => a.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Any())
            throw new DomainException($"Duplicate attribute names found: {string.Join(", ", duplicateNames)}");

        _attributes.Clear();
        _attributes.AddRange(attributes);
        SetModified(modifiedBy);
    }

    public void AddImage(ProductImage image)
    {
        Guard.Against.Null(image, nameof(image));
        _images.Add(image);
    }

    public void RemoveImage(ProductImage image)
    {
        Guard.Against.Null(image, nameof(image));
        _images.Remove(image);
    }

    public void UpdateSortOrder(int sortOrder, string? modifiedBy = null)
    {
        if (_sortOrder == sortOrder) return;

        _sortOrder = sortOrder;
        SetModified(modifiedBy);
    }

    public decimal CalculateProfit()
    {
        if (!_costPrice.HasValue) return 0;
        return _price - _costPrice.Value;
    }

    public decimal CalculateMargin()
    {
        if (!_costPrice.HasValue || _costPrice.Value == 0) return 0;
        return ((_price - _costPrice.Value) / _price) * 100;
    }

    public bool IsLowStock()
    {
        return _lowStockThreshold.HasValue && _stock <= _lowStockThreshold.Value;
    }

    public bool IsOutOfStock()
    {
        return _trackInventory && _stock <= 0 && !_allowBackorders;
    }

    public bool CanPurchase(int quantity)
    {
        if (!_trackInventory) return true;
        if (_allowBackorders) return true;
        return _stock >= quantity;
    }
}
