using Admin.Domain.Common;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;
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

        CreateMap<ProductCacheDto, Product>()
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Variants, opt => opt.Ignore())
            .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategory, opt => opt.Ignore())
          
            .AfterMap((src, dest) => {
            });

        CreateMap<CategoryCacheDto, Category>()
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());
        CreateMap<StockItem, StockItemCacheDto>();

        CreateMap<StockItemCacheDto, StockItem>()
            .ConstructUsing((src, ctx) => {
                // Use the parameterized constructor
                var stockItem = new StockItem(
                    productId: src.ProductId,
                    initialStock: src.CurrentStock,
                    lowStockThreshold: src.LowStockThreshold,
                    trackInventory: src.TrackInventory
                );

                // Set the ID correctly via reflection if needed
                typeof(StockItem).GetProperty("Id")?.SetValue(stockItem, src.Id);

                // Set creation/modification properties via reflection if needed
                typeof(AuditableEntity).GetProperty("CreatedAt")?.SetValue(stockItem, src.CreatedAt);
                typeof(AuditableEntity).GetProperty("CreatedBy")?.SetValue(stockItem, src.CreatedBy);
                typeof(AuditableEntity).GetProperty("LastModifiedAt")?.SetValue(stockItem, src.LastModifiedAt);
                typeof(AuditableEntity).GetProperty("LastModifiedBy")?.SetValue(stockItem, src.LastModifiedBy);
                typeof(AuditableEntity).GetProperty("IsActive")?.SetValue(stockItem, src.IsActive);

                return stockItem;
            })
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore())
            .ForMember(dest => dest.Reservations, opt => opt.Ignore());
    }
}