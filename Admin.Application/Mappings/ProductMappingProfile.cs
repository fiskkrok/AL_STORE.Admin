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
                opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Visibility, opt =>
                opt.MapFrom(src => src.Visibility.ToString()));

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.Price, opt =>
                opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt =>
                opt.MapFrom(src => src.Price.Currency));

        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductSeo, ProductSeoDto>();
        CreateMap<ProductDimensions, ProductDimensionsDto>();
        CreateMap<Category, CategoryDto>();
    }
}