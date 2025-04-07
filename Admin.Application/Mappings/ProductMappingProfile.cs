// Admin.Application/Mappings/ProductMappingProfile.cs
using Admin.Application.Categories.DTOs;
using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;
using Admin.Application.Products.Commands.CreateProduct;
using Admin.Domain.Enums;
using Admin.Domain.ValueObjects; // Add this import

using AutoMapper;

namespace Admin.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Price, opt =>
                opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt =>
                opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.CompareAtPrice, opt =>
                opt.MapFrom(src => src.CompareAtPrice != null ? src.CompareAtPrice.Amount : (decimal?)null))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()))
            .ForMember(dest => dest.Visibility, opt =>
                opt.MapFrom(src => src.Visibility.ToString().ToLowerInvariant()))
            .ForMember(dest => dest.Category, opt =>
                opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.SubCategory, opt =>
                opt.MapFrom(src => src.SubCategory))
            .ConstructUsing((src, context) => new ProductDto
            {
                Id = src.Id,
                Name = src.Name,
                Images = [],
                Variants = [],
                Attributes = [],
                Tags = []
            });

        // Add mapping for CreateProductCommand to Product
        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.CompareAtPrice, opt =>
                opt.MapFrom(src =>
                    src.CompareAtPrice.HasValue
                        ? Money.From(src.CompareAtPrice.Value, src.Currency)
                        : null)) // Use Money.From method
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => Enum.Parse<ProductStatus>(src.Status, true)))
            .ForMember(dest => dest.Visibility, opt =>
                opt.MapFrom(src => Enum.Parse<ProductVisibility>(src.Visibility, true)))
            .ForMember(dest => dest.Seo, opt =>
                opt.MapFrom(src =>
                    src.Seo != null
                        ? ProductSeo.Create(src.Seo.Title, src.Seo.Description, src.Seo.Keywords)
                        : null)) // Use ProductSeo.Create method
            .ForMember(dest => dest.Dimensions, opt =>
                opt.MapFrom(src =>
                    src.Dimensions != null
                        ? ProductDimensions.Create(src.Dimensions.Weight, src.Dimensions.Width, src.Dimensions.Height,
                            src.Dimensions.Length, src.Dimensions.Unit)
                        : null)) // Use ProductDimensions.Create method
            .ForMember(dest => dest.Variants, opt =>
                opt.MapFrom(src => src.Variants.Select(variantDto => new ProductVariant(
                    variantDto.Sku,
                    variantDto.Price,
                    variantDto.Currency,
                    variantDto.Stock,
                    Guid.NewGuid(),
                    variantDto.LowStockThreshold,
                    variantDto.CompareAtPrice,
                    variantDto.CostPrice,
                    variantDto.Barcode)
                {
                })))
            .ForMember(dest => dest.Attributes, opt =>
                opt.MapFrom(src => src.Attributes.Select(attributeDto =>
                    ProductAttribute.Create(attributeDto.Name, attributeDto.Value,
                        attributeDto.Type)))) // Use ProductAttribute.Create method
            .ForMember(dest => dest.Tags, opt =>
                opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.Images, opt => opt.Ignore()) // Ignore direct mapping
            .AfterMap((src, dest) =>
            {
                // Add images through the proper domain method
                foreach (var image in src.Images)
                {
                    dest.AddImage(image.Url, image.FileName, image.Size, null);
                }
                if (src.Variants != null)
                {
                    // If you have a proper AddVariant method:
                    foreach (var variantDto in src.Variants)
                    {
                        // Create a variant and then add it
                        var variant = new ProductVariant(
                            variantDto.Sku,
                            variantDto.Price,
                            variantDto.Currency,
                            variantDto.Stock,
                            dest.Id,
                            variantDto.LowStockThreshold,
                            variantDto.CompareAtPrice,
                            variantDto.CostPrice,
                            variantDto.Barcode
                        );

                        // If AddVariant exists, use it
                        if (typeof(Product).GetMethod("AddVariant") != null)
                        {
                            dest.AddVariant(variant);
                        }
                        else
                        {
                            // Otherwise access private field directly (not ideal but works as a workaround)
                            var variantsField = typeof(Product).GetField("_variants",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            if (variantsField != null && variantsField.GetValue(dest) is List<ProductVariant> variants)
                            {
                                variants.Add(variant);
                            }
                        }
                    }
                }

                // Add attributes
                if (src.Attributes != null)
                {
                    // Similar approach - either use AddAttribute method or access _attributes field directly
                    var attributesField = typeof(Product).GetField("_attributes",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (attributesField != null && attributesField.GetValue(dest) is List<ProductAttribute> attributes)
                    {
                        foreach (var attributeDto in src.Attributes)
                        {
                            attributes.Add(ProductAttribute.Create(
                                attributeDto.Name,
                                attributeDto.Value,
                                attributeDto.Type
                            ));
                        }
                    }
                }

                // Add tags
                if (src.Tags != null)
                {
                    foreach (var tag in src.Tags)
                    {
                        dest.AddTag(tag);
                    }
                }

            }); // Remove ProductTag mapping as it does not exist

        // Rest of the mappings remain the same
        CreateMap<Category, CategoryDto>();
        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.Price, opt =>
                opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt =>
                opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.CompareAtPrice, opt =>
                opt.MapFrom(src => src.CompareAtPrice != null ? src.CompareAtPrice.Amount : (decimal?)null))
            .ForMember(dest => dest.CostPrice, opt =>
                opt.MapFrom(src => src.CostPrice != null ? src.CostPrice.Amount : (decimal?)null))
            .ForMember(dest => dest.IsLowStock, opt =>
                opt.MapFrom(src => src.IsLowStock()))
            .ForMember(dest => dest.IsOutOfStock, opt =>
                opt.MapFrom(src => src.IsOutOfStock()))
            .ConstructUsing((src, context) => new ProductVariantDto
            {
                Id = src.Id,
                Sku = src.Sku,
                Price = src.Price.Amount,
                Currency = src.Price.Currency,
                // Other properties...
                Attributes = [],
                Images = [],
                ProductId = src.ProductId
            });
        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductSeo, ProductSeoDto>();
        CreateMap<ProductDimensions, ProductDimensionsDto>();
    }
}
