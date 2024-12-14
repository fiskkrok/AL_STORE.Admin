using Admin.Domain.Common.Exceptions;
using Admin.Domain.ValueObjects;
using Ardalis.GuardClauses;

public class ProductDimensions : ValueObject
{
    // Private parameterless constructor for EF Core
    private ProductDimensions() { }

    public decimal Weight { get; private set; }
    public decimal Width { get; private set; }
    public decimal Height { get; private set; }
    public decimal Length { get; private set; }
    public string Unit { get; private set; } = "cm";

    public static ProductDimensions Create(decimal weight, decimal width, decimal height, decimal length, string unit = "cm")
    {
        Guard.Against.NegativeOrZero(weight, nameof(weight));
        Guard.Against.NegativeOrZero(width, nameof(width));
        Guard.Against.NegativeOrZero(height, nameof(height));
        Guard.Against.NegativeOrZero(length, nameof(length));
        Guard.Against.NullOrWhiteSpace(unit, nameof(unit));

        if (unit != "cm" && unit != "inch")
            throw new DomainException("Unit must be either 'cm' or 'inch'");

        return new ProductDimensions
        {
            Weight = weight,
            Width = width,
            Height = height,
            Length = length,
            Unit = unit
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Weight;
        yield return Width;
        yield return Height;
        yield return Length;
        yield return Unit;
    }
}