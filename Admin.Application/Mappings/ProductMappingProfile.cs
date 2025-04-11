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
        CreateMap<ProductSeoDto, ProductSeo>()
            .ConstructUsing(src => ProductSeo.Create(
                src.Title,
                src.Description,
                src.Keywords ?? new List<string>()
            ));
        CreateMap<CreateProductCommand, Product>()
            .ConstructUsing((src, ctx) => new Product(
                name: src.Name,
                description: src.Description,
                price: src.Price,
                currency: src.Currency,
                sku: src.Sku,
                stock: src.Stock,
                categoryId: src.CategoryId,
                subCategoryId: src.SubCategoryId,
                createdBy: ctx.Items.ContainsKey("CurrentUser") ? ctx.Items["CurrentUser"] as string : null
            ))
            .ForMember(dest => dest.Seo, opt => opt.Ignore())
            .ForMember(dest => dest.Dimensions, opt => opt.Ignore())
            .ForMember(dest => dest.ShortDescription, opt => opt.Ignore())
            .ForMember(dest => dest.Barcode, opt => opt.Ignore())
            .ForMember(dest => dest.LowStockThreshold, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Visibility, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Variants, opt => opt.Ignore())
            .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            // Use AfterMap to properly set all the properties with private setters
            .AfterMap((src, dest, ctx) =>
            {
                string? currentUserId = ctx.Items.ContainsKey("CurrentUser")
                    ? ctx.Items["CurrentUser"] as string
                    : null;

                // Handle ShortDescription, Barcode, LowStockThreshold
                if (src.ShortDescription != null)
                    dest.UpdateShortDescription(src.ShortDescription, currentUserId);

                if (src.Barcode != null)
                    dest.UpdateBarcode(src.Barcode, currentUserId);

                if (src.LowStockThreshold.HasValue)
                    dest.UpdateLowStockThreshold(src.LowStockThreshold, currentUserId);

                // Status and Visibility
                dest.UpdateStatus(Enum.Parse<ProductStatus>(src.Status, true), currentUserId);
                dest.UpdateVisibility(Enum.Parse<ProductVisibility>(src.Visibility, true), currentUserId);

                // Handle CompareAtPrice
                if (src.CompareAtPrice.HasValue)
                    dest.UpdateCompareAtPrice(Money.From(src.CompareAtPrice.Value, src.Currency), currentUserId);

                // Handle Seo fields
                if (src.Seo != null)
                    dest.UpdateSeo(ProductSeo.Create(src.Seo.Title, src.Seo.Description, src.Seo.Keywords),
                        currentUserId);

                // Handle Dimensions
                if (src.Dimensions != null)
                    dest.UpdateDimensions(
                        ProductDimensions.Create(
                            src.Dimensions.Weight,
                            src.Dimensions.Width,
                            src.Dimensions.Height,
                            src.Dimensions.Length,
                            src.Dimensions.Unit
                        ),
                        currentUserId
                    );

                // Add images, variants, attributes, and tags
                foreach (var image in src.Images)
                    dest.AddImage(image.Url, image.FileName, image.Size, currentUserId);

                foreach (var variant in src.Variants)
                    dest.AddVariant(new ProductVariant(
                        variant.Sku,
                        variant.Price,
                        variant.Currency,
                        variant.Stock,
                        dest.Id, // Use the Product's ID
                        variant.LowStockThreshold,
                        variant.CompareAtPrice,
                        variant.CostPrice,
                        variant.Barcode
                    ));

                foreach (var attr in src.Attributes)
                    dest.AddAttribute(ProductAttribute.Create(attr.Name, attr.Value, attr.Type), currentUserId);

                foreach (var tag in src.Tags)
                    dest.AddTag(tag, currentUserId);
            });
    //.IgnoreMember(dest => dest.Images)
    //.IgnoreMember(dest => dest.Variants)
    //.IgnoreMember(dest => dest.Attributes)
    //.IgnoreMember(dest => dest.Tags);


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
