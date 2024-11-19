using Admin.Domain.Common;
using Admin.Domain.Common.Exceptions;

namespace Admin.Domain.Entities;

public class ProductImage : AuditableEntity
{
    private string _url = string.Empty;
    private string _fileName = string.Empty;
    private long _size;

    private ProductImage() { }

    public ProductImage(string url, string fileName, long size, Product product)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Image URL cannot be empty");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("Image file name cannot be empty");

        if (size <= 0)
            throw new DomainException("Image size must be greater than 0");

        _url = url;
        _fileName = fileName;
        _size = size;
        Product = product ?? throw new DomainException("Product is required");
        ProductId = product.Id;
    }

    public string Url => _url;
    public string FileName => _fileName;
    public long Size => _size;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
}