using Admin.Domain.ValueObjects;

namespace Admin.Domain.Entities;
public class ProductSeo : ValueObject
{
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<string> Keywords { get; private set; }

    private ProductSeo(string? title, string? description, IEnumerable<string> keywords)
    {
        Title = title;
        Description = description;
        Keywords = keywords.ToList().AsReadOnly();
    }

    public static ProductSeo Create(string? title, string? description, IEnumerable<string> keywords)
    {
        return new ProductSeo(title, description, keywords);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title ?? string.Empty;
        yield return Description ?? string.Empty;
        foreach (var keyword in Keywords)
            yield return keyword;
    }
}
