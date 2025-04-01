using Admin.Domain.Entities;
using Admin.Infrastructure.Services.Caching.DTOs;

using AutoMapper;

namespace Admin.Infrastructure.Services.Caching.Mappings;

public class CacheMappingProfile : Profile
{
    public CacheMappingProfile()
    {
        // Domain to Cache DTO mappings
        CreateMap<Product, ProductCacheDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<ProductVariant, ProductVariantCacheDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));

        CreateMap<ProductImage, ProductImageCacheDto>();

        CreateMap<Category, CategoryCacheDto>()
            .ForMember(dest => dest.SubCategoryIds, opt =>
                opt.MapFrom(src => src.SubCategories.Select(sc => sc.Id).ToList()));

        CreateMap<StockItem, StockItemCacheDto>();

        // Cache DTO to Domain mappings (if needed for hydration)
        // Note: These might not be needed depending on your usage pattern
        CreateMap<ProductCacheDto, Product>()
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategory, opt => opt.Ignore());

        CreateMap<CategoryCacheDto, Category>()
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());
    }
}