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
                        : null))
            .ForMember(dest => dest.LowStockThreshold, opt => opt.MapFrom(src => src.LowStockThreshold))
            .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.Barcode))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .IgnoreAllPropertiesWithAnInaccessibleSetter()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Variants, opt => opt.Ignore())
            .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore()) // Ignore direct mapping
            .AfterMap((src, dest) =>
            {
                // Add images through the proper domain method
                foreach (var image in src.Images ?? Enumerable.Empty<ProductImageDto>())
                {
                    dest.AddImage(image.Url, image.FileName, image.Size);
                }

                foreach (var var in src.Variants ?? Enumerable.Empty<ProductVariantDto>())
                {
                    dest.AddVariant(new ProductVariant(var.Sku, var.Price, var.Currency, var.Stock, var.ProductId, var.LowStockThreshold, var.CompareAtPrice, var.CostPrice, var.Barcode));
                }
                // Add attributes
                foreach (var Attributes in src.Attributes ?? Enumerable.Empty<ProductAttributeDto>())
                {
                    dest.AddAttribute(ProductAttribute.Create(Attributes.Name, Attributes.Value, Attributes.Type));
                }

                // Add tags
                foreach (var tag in src.Tags ?? Enumerable.Empty<string>())
                {
                    dest.AddTag(tag );
                }

            }); 

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
