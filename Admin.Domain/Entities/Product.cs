using System.Text.Json;
using System.Text.Json.Serialization;

using Admin.Domain.Common;
using Admin.Domain.Common.Exceptions;
using Admin.Domain.Enums;
using Admin.Domain.Events.Product;
using Admin.Domain.ValueObjects;

using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
[JsonConverter(typeof(ProductJsonConverter))]
public class Product : AuditableEntity
{
    // Collections still need explicit backing fields as they're initialized
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductAttribute> _attributes = [];
    private readonly List<string> _tags = [];

    // Money-related fields need to stay as backing fields due to the value object pattern
    private decimal _price;
    private string _currency = "USD";
    private decimal? _compareAtPrice;

    private Product() { }

    public Product(
        string name,
        string description,
        decimal price,
        string currency,
        string sku,
        int stock,
        Guid categoryId,
        Guid? id = null,
        Guid? productTypeId = null,
        Guid? subCategoryId = null,
        string? createdBy = null) : base(id)

    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.NullOrWhiteSpace(sku, nameof(sku));
        Guard.Against.NegativeOrZero(price, nameof(price));
        Guard.Against.Negative(stock, nameof(stock));
        Guard.Against.NullOrWhiteSpace(currency, nameof(currency));
        Guard.Against.InvalidInput(currency, nameof(currency), c => c.Length == 3);
        Guard.Against.Null(categoryId, nameof(categoryId));

        Id = id ?? Guid.NewGuid(); // Set the ID if provided, otherwise generate new
        Name = name;
        Description = description;
        _price = price;
        _currency = currency;
        Sku = sku;
        Stock = stock;
        CategoryId = categoryId;
        ProductTypeId = productTypeId;
        SubCategoryId = subCategoryId;
        SetCreated(createdBy);
        AddDomainEvent(new ProductCreatedDomainEvent(this));
    }

    // Using new field keyword for simple properties
    public string Name
    {
        get => field;
        private set
        {
            field = value;
            Slug = GenerateSlug(value); // Update slug when name changes
        }
    }

    public string Slug { get; private set; }

    public string Description
    {
        get => field;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Description));
    }

    public string? ShortDescription
    {
        get => field;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(ShortDescription));
    }

    public string Sku
    {
        get => field;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Sku));
    }

    public string? Barcode
    {
        get => field;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Barcode));
    }
    public int? LowStockThreshold
    {
        get => field;
        private set => field = Guard.Against.Null(value, nameof(LowStockThreshold));
    }

    public ProductType? ProductType
    {
        get => field;
        private set => field = Guard.Against.Null(value, nameof(ProductType));
    }
    public Guid? ProductTypeId
    {
        get => field;
        private set => field = Guard.Against.Null(value, nameof(ProductTypeId));
    }
    
    // Complex properties still using explicit backing fields
    public Money Price => Money.From(_price, _currency);
    public Money? CompareAtPrice => _compareAtPrice.HasValue ? Money.From(_compareAtPrice.Value, _currency) : null;

   
    [Obsolete("Use StockItem entity for inventory management instead")]
    public int Stock
    {
        get => field;
        private set => field = Guard.Against.Negative(value, nameof(Stock));
    }
  
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public ProductVisibility Visibility { get; private set; } = ProductVisibility.Hidden;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public Guid? SubCategoryId { get; private set; }
    public Category? SubCategory { get; private set; }
    public ProductSeo? Seo { get; private set; }
    public ProductDimensions? Dimensions { get; private set; }

    // Collection properties
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();
    public bool IsArchived { get; private set; }

    public void Update(
        string name,
        string description,
        Money price,
        Category category,
        Category? subCategory = null,
        string? modifiedBy = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));
        Guard.Against.Null(price, nameof(price));
        Guard.Against.Null(category, nameof(category));

        if (Name == name &&
            Description == description &&
            _price == price.Amount &&
            CategoryId == category.Id &&
            SubCategoryId == subCategory?.Id) return;

        Name = name;
        Description = description;
        _price = price.Amount;
        _currency = price.Currency;
        Category = category;
        CategoryId = category.Id;

        if (subCategory != null)
        {
            SubCategory = subCategory;
            SubCategoryId = subCategory.Id;
        }
        else
        {
            SubCategory = null;
            SubCategoryId = null;
        }

        SetModified(modifiedBy);
        AddDomainEvent(new ProductUpdatedDomainEvent(this));
    }


    public void UpdatePrice(Money newPrice, string? modifiedBy = null)
    {
        if (_price == newPrice.Amount && _currency == newPrice.Currency)
            return;

        _price = newPrice.Amount;
        _currency = newPrice.Currency;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductUpdatedDomainEvent(this));
    }
    public void UpdateStock(int newStock, string? modifiedBy = null)
    {
        Guard.Against.Negative(newStock, nameof(newStock));

        if (Stock == newStock) return;

        var oldStock = Stock;
        Stock = newStock;
        SetModified(modifiedBy);

        AddDomainEvent(new ProductStockUpdatedDomainEvent(this, oldStock, newStock));
    }
    public void SyncStockFromStockItem(StockItem stockItem)
    {
        if (stockItem == null || stockItem.ProductId != Id)
            return;

        // Keep the legacy Stock property synchronized
        Stock = stockItem.CurrentStock;
    }
    public void AddImage(string url, string fileName, long size, string? modifiedBy = null)
    {
        Guard.Against.NullOrWhiteSpace(url, nameof(url));
        Guard.Against.NullOrWhiteSpace(fileName, nameof(fileName));
        Guard.Against.NegativeOrZero(size, nameof(size));

        var image = new ProductImage(url, fileName, size, this.Id);
        _images.Add(image);

        SetModified(modifiedBy);
        AddDomainEvent(new ProductImageAddedDomainEvent(this, image));
    }
    public void SetProductType(ProductType? productType, string? modifiedBy = null)
    {
        ProductType = productType;
        ProductTypeId = productType?.Id;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductTypeUpdatedDomainEvent(this, productType));
    }
    public void UpdateShortDescription(string? shortDescription, string? modifiedBy = null)
    {
        if (ShortDescription == shortDescription) return;
        ShortDescription = shortDescription;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductShortDescriptionUpdatedDomainEvent(this, shortDescription));
    }

    public void UpdateBarcode(string? barcode, string? modifiedBy = null)
    {
        if (Barcode == barcode) return;
        Barcode = barcode;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductBarcodeUpdatedDomainEvent(this, barcode));
    }

    public void UpdateLowStockThreshold(int? threshold, string? modifiedBy = null)
    {
        if (LowStockThreshold == threshold) return;
        LowStockThreshold = threshold;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductLowStockThresholdUpdatedDomainEvent(this, threshold));
    }

    public void UpdateCompareAtPrice(Money? compareAtPrice, string? modifiedBy = null)
    {
        if (CompareAtPrice == compareAtPrice) return;
        _compareAtPrice = compareAtPrice?.Amount;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductCompareAtPriceUpdatedDomainEvent(this, compareAtPrice));
    }
    public void RemoveImage(ProductImage image, string? modifiedBy = null)
    {
        Guard.Against.Null(image, nameof(image));

        if (_images.Remove(image))
        {
            SetModified(modifiedBy);
            AddDomainEvent(new ProductImageRemovedDomainEvent(this, image));
        }
    }

    public void Delete(string? modifiedBy = null)
    {
        if (!IsActive) return;
        
        SetInactive(modifiedBy);
        AddDomainEvent(new ProductDeletedDomainEvent(this));
    }

    public void Archive(string? modifiedBy = null)
    {
        if (IsArchived) return;
        IsArchived = true;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductArchivedDomainEvent(this));
    }

    public void Restore(string? modifiedBy = null)
    {
        if (!IsArchived) return;
        IsArchived = false;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductRestoredDomainEvent(this));
    }

    public void AddTag(string tag, string? modifiedBy = null)
    {
        Guard.Against.NullOrWhiteSpace(tag, nameof(tag));
        if (_tags.Contains(tag)) return;
        _tags.Add(tag);
        SetModified(modifiedBy);
        AddDomainEvent(new ProductTagAddedDomainEvent(this, tag));
    }

    public void RemoveTag(string tag, string? modifiedBy = null)
    {
        Guard.Against.NullOrWhiteSpace(tag, nameof(tag));
        if (_tags.Remove(tag))
        {
            SetModified(modifiedBy);
            AddDomainEvent(new ProductTagRemovedDomainEvent(this, tag));
        }
    }
    public void AddAttribute(ProductAttribute attribute, string? modifiedBy = null)
    {
        Guard.Against.Null(attribute, nameof(attribute));
        if (_attributes.Contains(attribute)) return;
        _attributes.Add(attribute);
        SetModified(modifiedBy);
        AddDomainEvent(new ProductAttributeAddedDomainEvent(this, attribute));
    }


    public void UpdateSeo(ProductSeo seo, string? modifiedBy = null)
    {
        Guard.Against.Null(seo, nameof(seo));
        if (Seo == seo) return;
        Seo = seo;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductSeoUpdatedDomainEvent(this, seo));
    }

    public void UpdateDimensions(ProductDimensions dimensions, string? modifiedBy = null)
    {
        Guard.Against.Null(dimensions, nameof(dimensions));
        if (Dimensions == dimensions) return;
        Dimensions = dimensions;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductDimensionsUpdatedDomainEvent(this, dimensions));
    }

    public void UpdateStatus(ProductStatus status, string? modifiedBy = null)
    {
        if (Status == status) return;
        Status = status;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductStatusUpdatedDomainEvent(this, status));
    }

    public void UpdateVisibility(ProductVisibility visibility, string? modifiedBy = null)
    {
        if (Visibility == visibility) return;
        Visibility = visibility;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductVisibilityUpdatedDomainEvent(this, visibility));
    }


    private static string GenerateSlug(string name) =>
        name.ToLower().Replace(" ", "-");


    public void AddVariant(ProductVariant variant)
    {
        Guard.Against.Null(variant, nameof(variant));

        if (_variants.Any(v => v.Sku == variant.Sku))
            throw new DomainException($"Variant with SKU '{variant.Sku}' already exists");

        _variants.Add(variant);
        AddDomainEvent(new ProductVariantAddedDomainEvent(this, variant));
    }
}

public record ProductBarcodeUpdatedDomainEvent(Product Product, string? Barcode) : DomainEvent;

public record ProductLowStockThresholdUpdatedDomainEvent(Product Product, int? Threshold) : DomainEvent;

public record ProductCompareAtPriceUpdatedDomainEvent(Product Product, Money? CompareAtPrice) : DomainEvent;

public record ProductTypeUpdatedDomainEvent(Product Product, ProductType? ProductType) : DomainEvent;

public record ProductShortDescriptionUpdatedDomainEvent(Product Product, string? ShortDescription) : DomainEvent;

public record ProductAttributeAddedDomainEvent(Product Product, ProductAttribute Attribute) : DomainEvent;

public record ProductVariantAddedDomainEvent(Product Product, ProductVariant variant) : DomainEvent;

public record ProductArchivedDomainEvent(Product Product) : DomainEvent;

public record ProductTagAddedDomainEvent(Product Product, string Tag) : DomainEvent;

public record ProductTagRemovedDomainEvent(Product Product, string Tag) : DomainEvent;

public record ProductSeoUpdatedDomainEvent(Product Product, ProductSeo Seo) : DomainEvent;

public record ProductStatusUpdatedDomainEvent(Product Product, ProductStatus Status) : DomainEvent;

public record ProductDimensionsUpdatedDomainEvent(Product Product, ProductDimensions Dimensions) : DomainEvent;

public record ProductVisibilityUpdatedDomainEvent(Product Product, ProductVisibility Visibility) : DomainEvent;

public record ProductRestoredDomainEvent(Product Product) : DomainEvent;

