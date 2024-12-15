using System.Text.Json.Serialization;

using Admin.Domain.Common;
using Ardalis.GuardClauses;

namespace Admin.Domain.Entities;

// ProductImage.cs
public class ProductImage : AuditableEntity
{
    private string _url = string.Empty;
    private string _fileName = string.Empty;
    private string? _alt;
    private long _size;
    private int _sortOrder;
    private bool _isPrimary;

    // Private constructor for EF Core
    private ProductImage()
    {
    }

    public ProductImage(
        string url,
        string fileName,
        long size,
        Guid productId,
        bool isPrimary = false,
        string? alt = null)
    {
        Guard.Against.NullOrWhiteSpace(url, nameof(url));
        Guard.Against.NullOrWhiteSpace(fileName, nameof(fileName));
        Guard.Against.NegativeOrZero(size, nameof(size));

        _url = url;
        _fileName = fileName;
        _size = size;
        _alt = alt;
        _isPrimary = isPrimary;
        ProductId = productId;
    }

    public string Url => _url;
    public string FileName => _fileName;
    public string? Alt => _alt;
    public long Size => _size;
    public int SortOrder => _sortOrder;
    public bool IsPrimary => _isPrimary;
    public Guid ProductId { get; private set; }
    [JsonIgnore]
    public Product Product { get; private set; } = null!;

    public void UpdateSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
    }

    public void SetAsPrimary(bool isPrimary = true)
    {
        _isPrimary = isPrimary;
    }

    public void UpdateAlt(string? alt)
    {
        _alt = alt;
    }
}
