// Admin.Domain/Entities/ProductType.cs
using System.Collections.Generic;

using Admin.Domain.Common;

namespace Admin.Domain.Entities;

public class ProductType : AuditableEntity
{
    private ProductType() { } // For EF Core

    public ProductType(string id, string name, string? description = null, string? icon = null)
    {
        Id = new Guid(id); // Note: Using GUID for consistency, but allowing string input
        Name = name;
        Description = description;
        Icon = icon;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string AttributesJson { get; private set; } = "[]";

    public void UpdateAttributes(string attributesJson)
    {
        AttributesJson = attributesJson;
    }

    public void Update(string name, string? description, string? icon)
    {
        Name = name;
        Description = description;
        Icon = icon;
    }
}