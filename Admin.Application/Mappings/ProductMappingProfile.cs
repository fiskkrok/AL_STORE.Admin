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
            .ForMember(dest => dest.Category, opt =>
                opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.SubCategory, opt =>
                opt.MapFrom(src => src.SubCategory))
            .ForMember(dest => dest.Images, opt =>
                opt.MapFrom(src => src.Images));

        CreateMap<Category, CategoryDto>();
        CreateMap<ProductImage, ProductImageDto>();
    }
}