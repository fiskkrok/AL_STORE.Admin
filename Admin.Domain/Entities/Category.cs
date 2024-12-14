using Admin.Domain.Common;
using Admin.Domain.Common.Exceptions;
using Admin.Domain.Events;

namespace Admin.Domain.Entities;

public class Category : AuditableEntity
{
    private readonly List<Category> _subCategories = new();
    private readonly List<Product> _products = new();
    private string _name = string.Empty;
    private string _description = string.Empty;

    private Category() { }  // For EF Core

    public Category(string name, string description)
    {
        UpdateCore(name, description);
        AddDomainEvent(new CategoryCreatedDomainEvent(this));
    }

    public string Name => _name;
    public string Description => _description;
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Add this property for the parent category relationship
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; private set; }

    public void Update(string name, string description, string? modifiedBy = null)
    {
        UpdateCore(name, description);
        SetModified(modifiedBy);
        AddDomainEvent(new CategoryUpdatedDomainEvent(this));
    }

    private void UpdateCore(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name cannot be empty");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Category description cannot be empty");

        _name = name;
        _description = description;
    }

    public void Delete(string? modifiedBy = null)
    {
        SetInactive(modifiedBy);
        AddDomainEvent(new CategoryDeletedDomainEvent(this));
    }
}