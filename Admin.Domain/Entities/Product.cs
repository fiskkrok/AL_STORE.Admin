using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Admin.Domain.Common;
using Admin.Domain.Enums;
using Admin.Domain.Events;
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

    public string? ShortDescription { get; private set; }

    public string Sku
    {
        get => field;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Sku));
    }

    public string? Barcode { get; private set; }

    // Complex properties still using explicit backing fields
    public Money Price => Money.From(_price, _currency);
    public Money? CompareAtPrice => _compareAtPrice.HasValue ? Money.From(_compareAtPrice.Value, _currency) : null;

    public int Stock
    {
        get => field;
        private set => field = Guard.Against.Negative(value, nameof(Stock));
    }

    public int? LowStockThreshold { get; private set; }
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

    public void UpdatePrice(Money newPrice, string? modifiedBy = null)
    {
        if (_price == newPrice.Amount && _currency == newPrice.Currency)
            return;

        _price = newPrice.Amount;
        _currency = newPrice.Currency;
        SetModified(modifiedBy);
        AddDomainEvent(new ProductUpdatedDomainEvent(this));
    }

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

    public void UpdateStock(int newStock, string? modifiedBy = null)
    {
        Guard.Against.Negative(newStock, nameof(newStock));

        if (Stock == newStock) return;

        var oldStock = Stock;
        Stock = newStock;
        SetModified(modifiedBy);

        AddDomainEvent(new ProductStockUpdatedDomainEvent(this, oldStock, newStock));
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
        throw new NotImplementedException();
    }
}

public record ProductArchivedDomainEvent(Product Product) : DomainEvent;

public record ProductTagAddedDomainEvent(Product Product, string Tag) : DomainEvent;

public record ProductTagRemovedDomainEvent(Product Product, string Tag) : DomainEvent;

public record ProductSeoUpdatedDomainEvent(Product Product, ProductSeo Seo) : DomainEvent;

public record ProductStatusUpdatedDomainEvent(Product Product, ProductStatus Status) : DomainEvent;

public record ProductDimensionsUpdatedDomainEvent(Product Product, ProductDimensions Dimensions) : DomainEvent;

public record ProductVisibilityUpdatedDomainEvent(Product Product, ProductVisibility Visibility) : DomainEvent;

public record ProductRestoredDomainEvent(Product Product) : DomainEvent;

public class ProductJsonConverter : JsonConverter<Product>
{
    public override Product? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        using (JsonDocument document = JsonDocument.ParseValue(ref reader))
        {
            var root = document.RootElement;

            try
            {
                // Get the ID first
                var id = root.GetProperty("id").GetGuid();

                // Get other required properties for constructor
                var name = root.GetProperty("name").GetString() ?? "";
                var description = root.GetProperty("description").GetString() ?? "";
                var price = root.GetProperty("price").GetProperty("amount").GetDecimal();
                var currency = root.GetProperty("currency").GetString() ?? "USD";
                var sku = root.GetProperty("sku").GetString() ?? "";
                var stock = root.GetProperty("stock").GetInt32();
                var categoryId = root.GetProperty("categoryId").GetGuid();

                var product = new Product(
                    name: name,
                    description: description,
                    price: price,
                    currency: currency,
                    sku: sku,
                    stock: stock,
                    categoryId: categoryId,
                    id: id, 
                    createdBy: null
                );


             
                // Handle optional properties
                if (root.TryGetProperty("shortDescription", out var shortDesc))
                {
                    typeof(Product).GetProperty("ShortDescription")?.SetValue(product, shortDesc.GetString());
                }

                if (root.TryGetProperty("status", out var status) && Enum.TryParse<ProductStatus>(status.GetString(), out var statusEnum))
                {
                    typeof(Product).GetProperty("Status")?.SetValue(product, statusEnum);
                }

                if (root.TryGetProperty("visibility", out var visibility) && Enum.TryParse<ProductVisibility>(visibility.GetString(), out var visibilityEnum))
                {
                    typeof(Product).GetProperty("Visibility")?.SetValue(product, visibilityEnum);
                }

                if (root.TryGetProperty("subCategoryId", out var subCategoryId))
                {
                    typeof(Product).GetProperty("SubCategoryId")?.SetValue(product, subCategoryId.GetGuid());
                }

                // Handle collections if present
                if (root.TryGetProperty("images", out var images))
                {
                    foreach (var image in images.EnumerateArray())
                    {
                        var url = image.GetProperty("url").GetString();
                        var fileName = image.GetProperty("fileName").GetString();
                        var size = image.GetProperty("size").GetInt64();

                        if (url != null && fileName != null)
                        {
                            product.AddImage(url, fileName, size);
                        }
                    }
                }

                return product;
            }
            catch (Exception ex)
            {
                throw new JsonException($"Error deserializing Product: {ex.Message}", ex);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, Product value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write basic properties
        writer.WriteString("id", value.Id);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);

        // Write price as an object
        writer.WriteStartObject("price");
        writer.WriteNumber("amount", value.Price.Amount);
        writer.WriteString("currency", value.Price.Currency);
        writer.WriteEndObject();

        writer.WriteString("currency", value.Price.Currency);
        writer.WriteString("sku", value.Sku);
        writer.WriteNumber("stock", value.Stock);
        writer.WriteString("categoryId", value.CategoryId);
        writer.WriteString("status", value.Status.ToString());
        writer.WriteString("visibility", value.Visibility.ToString());

        // Write optional properties
        if (value.ShortDescription != null)
            writer.WriteString("shortDescription", value.ShortDescription);

        if (value.SubCategoryId.HasValue)
            writer.WriteString("subCategoryId", value.SubCategoryId.Value);

        // Write collections
        writer.WriteStartArray("images");
        foreach (var image in value.Images)
        {
            writer.WriteStartObject();
            writer.WriteString("url", image.Url);
            writer.WriteString("fileName", image.FileName);
            writer.WriteNumber("size", image.Size);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}