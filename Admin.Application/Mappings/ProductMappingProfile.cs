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
                opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Visibility, opt =>
                opt.MapFrom(src => src.Visibility.ToString()))
            .ForMember(dest => dest.Category, opt =>
                opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.SubCategory, opt =>
                opt.MapFrom(src => src.SubCategory))
            .AfterMap((src, dest) => {
                // Ensure Category is never null
                if (dest.Category == null)
                {
                    dest.Category = new CategoryDto
                    {
                        Id = src.CategoryId,
                        Name = "Unknown",
                        Description = "Category not found"
                    };
                }
            });

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt =>
                opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt =>
                opt.MapFrom(src => src.Description));

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.Price, opt =>
                opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt =>
                opt.MapFrom(src => src.Price.Currency));

        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductSeo, ProductSeoDto>();
        CreateMap<ProductDimensions, ProductDimensionsDto>();
    }
}