// Admin.Application/Products/Queries/GetProductTypesQuery.cs
using Admin.Application.Common.Models;
using Admin.Application.Products.DTOs;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Admin.Application.Products.Queries;

public record GetProductTypesQuery : IRequest<Result<List<ProductTypeDto>>>;

public class GetProductTypesQueryHandler : IRequestHandler<GetProductTypesQuery, Result<List<ProductTypeDto>>>
{
    private readonly ILogger<GetProductTypesQueryHandler> _logger;

    public GetProductTypesQueryHandler(ILogger<GetProductTypesQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result<List<ProductTypeDto>>> Handle(GetProductTypesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // For now, we'll return a static list of product types
            // In a real application, you would fetch this from a database or configuration
            var productTypes = GetPredefinedProductTypes();

            return Task.FromResult(Result<List<ProductTypeDto>>.Success(productTypes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product types");
            return Task.FromResult(Result<List<ProductTypeDto>>.Failure(
                new Error("ProductTypes.GetFailed", "Failed to retrieve product types")));
        }
    }

    private static List<ProductTypeDto> GetPredefinedProductTypes()
    {
        return
        [
            new()
            {
                Id = "clothing",
                Name = "Clothing",
                Description = "Apparel items including shirts, pants, dresses, etc.",
                Icon = "checkroom",
                Attributes =
                [
                    new()
                    {
                        Id = "size",
                        Name = "Sizes",
                        Type = "multiselect",
                        IsRequired = true,
                        Options =
                        [
                            new() { Label = "XS", Value = "XS" },
                            new() { Label = "S", Value = "S" },
                            new() { Label = "M", Value = "M" },
                            new() { Label = "L", Value = "L" },
                            new() { Label = "XL", Value = "XL" },
                            new() { Label = "XXL", Value = "XXL" }
                        ],
                        DisplayOrder = 1,
                        IsFilterable = true,
                        IsComparable = false
                    },

                    new()
                    {
                        Id = "color",
                        Name = "Colors",
                        Type = "color",
                        IsRequired = true,
                        DisplayOrder = 2,
                        IsFilterable = true,
                        IsComparable = false
                    },

                    new()
                    {
                        Id = "material",
                        Name = "Material",
                        Type = "text",
                        IsRequired = false,
                        DisplayOrder = 3,
                        IsFilterable = true,
                        IsComparable = true
                    },

                    new()
                    {
                        Id = "gender",
                        Name = "Gender",
                        Type = "select",
                        IsRequired = true,
                        Options =
                        [
                            new() { Label = "Men", Value = "men" },
                            new() { Label = "Women", Value = "women" },
                            new() { Label = "Unisex", Value = "unisex" },
                            new() { Label = "Boys", Value = "boys" },
                            new() { Label = "Girls", Value = "girls" }
                        ],
                        DisplayOrder = 4,
                        IsFilterable = true,
                        IsComparable = false
                    }
                ]
            },

            new()
            {
                Id = "electronics",
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                Icon = "devices",
                Attributes =
                [
                    new()
                    {
                        Id = "brand",
                        Name = "Brand",
                        Type = "text",
                        IsRequired = true,
                        DisplayOrder = 1,
                        IsFilterable = true,
                        IsComparable = true
                    },

                    new()
                    {
                        Id = "model",
                        Name = "Model",
                        Type = "text",
                        IsRequired = true,
                        DisplayOrder = 2,
                        IsFilterable = true,
                        IsComparable = true
                    },

                    new()
                    {
                        Id = "warranty",
                        Name = "Warranty Period (months)",
                        Type = "number",
                        IsRequired = false,
                        DisplayOrder = 3,
                        IsFilterable = true,
                        IsComparable = true
                    }
                ]
            },

            new()
            {
                Id = "books",
                Name = "Books",
                Description = "Books, publications, and literature",
                Icon = "menu_book",
                Attributes =
                [
                    new()
                    {
                        Id = "author",
                        Name = "Author",
                        Type = "text",
                        IsRequired = true,
                        DisplayOrder = 1,
                        IsFilterable = true,
                        IsComparable = false
                    },

                    new()
                    {
                        Id = "isbn",
                        Name = "ISBN",
                        Type = "text",
                        IsRequired = true,
                        DisplayOrder = 2,
                        IsFilterable = false,
                        IsComparable = false,
                        Validation = new ProductTypeAttributeValidationDto
                        {
                            Pattern =
                                "^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$",
                            Message = "Please enter a valid ISBN"
                        }
                    },

                    new()
                    {
                        Id = "pages",
                        Name = "Page Count",
                        Type = "number",
                        IsRequired = false,
                        DisplayOrder = 3,
                        IsFilterable = false,
                        IsComparable = true
                    }
                ]
            }
        ];
    }
}