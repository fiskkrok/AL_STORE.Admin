using Admin.Domain.ValueObjects;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;
public class ProductAttribute : ValueObject
{
    public string Name { get; private set; }
    public string Value { get; private set; }
    public string Type { get; private set; }

    private ProductAttribute(string name, string value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    public static ProductAttribute Create(string name, string value, string type)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(value, nameof(value));
        Guard.Against.NullOrWhiteSpace(type, nameof(type));

        return new ProductAttribute(name, value, type);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Value;
        yield return Type;
    }
}