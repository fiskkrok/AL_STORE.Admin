using Admin.Application.Categories.DTOs;
using Admin.Domain.Entities;
using AutoMapper;

namespace Admin.Application.Mappings;
public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count))
            .ForMember(dest => dest.ParentCategory,
                opt => opt.MapFrom(src => src.ParentCategory))
            .ForMember(dest => dest.SubCategories,
                opt => opt.MapFrom(src => src.SubCategories))
            .AfterMap((src, dest) =>
            {
                // Ensure circular references are handled properly
                if (dest.ParentCategory != null)
                {
                    dest.ParentCategory.SubCategories.Clear();
                    dest.ParentCategory.ParentCategory = null;
                }

                foreach (var sub in dest.SubCategories)
                {
                    sub.ParentCategory = null;
                }
            });
    }
}
