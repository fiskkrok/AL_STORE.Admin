using Admin.Domain.Common;
using Admin.Domain.Common.Exceptions;
using Admin.Domain.Events.Category;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;

public partial class Category : AuditableEntity
{
    private readonly List<Category> _subCategories = new();
    private readonly List<Product> _products = new();
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _slug = string.Empty;
    private int _sortOrder;
    private string? _metaTitle;
    private string? _metaDescription;
    private string? _imageUrl;

    private Category() { } // For EF Core

    public Category(
        string name,
        string description,
        string? imageUrl = null,
        Category? parent = null)
    {
        UpdateCore(name, description);
        _imageUrl = imageUrl;

        if (parent != null)
        {
            ValidateParent(parent);
            ParentCategory = parent;
            ParentCategoryId = parent.Id;
        }

        AddDomainEvent(new CategoryCreatedDomainEvent(this));
    }

    // Properties
    public string Name => _name;
    public string Description => _description;
    public string Slug => _slug;
    public int SortOrder => _sortOrder;
    public string? MetaTitle => _metaTitle;
    public string? MetaDescription => _metaDescription;
    public string? ImageUrl => _imageUrl;
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Public methods
    public void Update(
        string name,
        string description,
        string? metaTitle = null,
        string? metaDescription = null,
        string? modifiedBy = null)
    {
        UpdateCore(name, description);
        _metaTitle = metaTitle;
        _metaDescription = metaDescription;
        SetModified(modifiedBy);

        AddDomainEvent(new CategoryUpdatedDomainEvent(this));
    }

    public void UpdateImage(string? imageUrl, string? modifiedBy = null)
    {
        if (_imageUrl == imageUrl) return;

        _imageUrl = imageUrl;
        SetModified(modifiedBy);

        AddDomainEvent(new CategoryImageUpdatedDomainEvent(this, imageUrl));
    }

    public void UpdateParent(Category? newParent, string? modifiedBy = null)
    {
        if (newParent?.Id == ParentCategoryId) return;

        var oldParent = ParentCategory;

        if (newParent != null)
        {
            ValidateParent(newParent);
            ParentCategory = newParent;
            ParentCategoryId = newParent.Id;
            UpdateMaterializedPath(newParent.MaterializedPath);
        }
        else
        {
            ParentCategory = null;
            ParentCategoryId = null;
            UpdateMaterializedPath(null);
        }

        SetModified(modifiedBy);
        AddDomainEvent(new CategoryParentChangedDomainEvent(this, oldParent, newParent));
    }

    public void UpdateSortOrder(int sortOrder, string? modifiedBy = null)
    {
        if (_sortOrder == sortOrder) return;

        _sortOrder = sortOrder;
        SetModified(modifiedBy);

        AddDomainEvent(new CategorySortOrderChangedDomainEvent(this, sortOrder));
    }

    public void AddSubCategory(Category category, string? modifiedBy = null)
    {
        Guard.Against.Null(category, nameof(category));

        if (_subCategories.Any(c => c.Id == category.Id))
            throw new DomainException($"Subcategory with ID {category.Id} already exists");

        if (category.IsAncestorOf(this))
            throw new DomainException("Cannot add an ancestor as a subcategory (circular reference)");

        category.UpdateParent(this, modifiedBy);
        _subCategories.Add(category);
    }

    public void RemoveSubCategory(Category category, string? modifiedBy = null)
    {
        Guard.Against.Null(category, nameof(category));

        if (_subCategories.Remove(category))
        {
            category.UpdateParent(null, modifiedBy);
        }
    }

    public void Delete(string? modifiedBy = null)
    {
        if (!IsActive) return;

        // Check if category has products
        if (Products.Any())
            throw new DomainException("Cannot delete category with associated products");

        // Recursively delete subcategories
        foreach (var subCategory in SubCategories.ToList())
        {
            subCategory.Delete(modifiedBy);
        }

        SetInactive(modifiedBy);
        AddDomainEvent(new CategoryDeletedDomainEvent(this));
    }

    // Helper methods
    private void UpdateCore(string name, string description)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        _name = name;
        _description = description;
        _slug = GenerateSlug(name);
    }

    private void ValidateParent(Category parent)
    {
        if (parent.Id == Id)
            throw new DomainException("Category cannot be its own parent");

        if (parent.IsAncestorOf(this))
            throw new DomainException("Cannot create circular reference in category hierarchy");
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("@", "at")
            .Replace("$", "dollar");
    }
}

public partial class Category
{
    private string? _materializedPath;

    public string MaterializedPath
    {
        get => _materializedPath ?? Id.ToString();
        private set => _materializedPath = value;
    }

    public void UpdateMaterializedPath(string? parentPath = null)
    {
        var path = parentPath != null
            ? $"{parentPath}/{Id}"
            : Id.ToString();

        if (_materializedPath != path)
        {
            _materializedPath = path;

            // Update all subcategories recursively
            foreach (var subCategory in _subCategories)
            {
                subCategory.UpdateMaterializedPath(_materializedPath);
            }
        }
    }

    public int GetLevel()
    {
        return MaterializedPath.Count(c => c == '/');
    }

    public bool IsDescendantOf(Category possibleAncestor)
    {
        return MaterializedPath.StartsWith(possibleAncestor.MaterializedPath + "/");
    }

    public bool IsAncestorOf(Category possibleDescendant)
    {
        return possibleDescendant.MaterializedPath.StartsWith(MaterializedPath + "/");
    }
}