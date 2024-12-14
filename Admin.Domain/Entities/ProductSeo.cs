using Admin.Domain.ValueObjects;

namespace Admin.Domain.Entities;

public class ProductSeo : ValueObject
{
    private readonly List<string> _keywords = new();

    // Private parameterless constructor for EF Core
    private ProductSeo() { }

    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<string> Keywords => _keywords.AsReadOnly();

    public static ProductSeo Create(string? title, string? description, IEnumerable<string> keywords)
    {
        var seo = new ProductSeo
        {
            Title = title,
            Description = description
        };

        if (keywords != null)
        {
            seo._keywords.AddRange(keywords);
        }

        return seo;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title ?? string.Empty;
        yield return Description ?? string.Empty;
        foreach (var keyword in Keywords)
            yield return keyword;
    }
}