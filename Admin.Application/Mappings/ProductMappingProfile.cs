// Admin.Application/Mappings/ProductMappingProfile.cs
using Admin.Application.Categories.DTOs;
using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;

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
                Images = new List<ProductImageDto>(),
                Variants = new List<ProductVariantDto>(),
                Attributes = new List<ProductAttributeDto>(),
                Tags = new List<string>()
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
                Attributes = new List<ProductAttributeDto>(),
                Images = new List<ProductImageDto>(),
                ProductId = src.ProductId
            });
        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductSeo, ProductSeoDto>();
        CreateMap<ProductDimensions, ProductDimensionsDto>();
    }
}
