// Admin.Application/Mappings/CategoryMappingProfile.cs
using Admin.Application.Categories.DTOs;
using Admin.Domain.Entities;

using AutoMapper;

using System.Collections.Generic;

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
            .ConstructUsing((src, context) => {
                // Create with initial values
                return new CategoryDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    Description = src.Description,
                    Slug = src.Slug,
                    SortOrder = src.SortOrder,
                    MetaTitle = src.MetaTitle,
                    MetaDescription = src.MetaDescription,
                    ImageUrl = src.ImageUrl,
                    ParentCategoryId = src.ParentCategoryId,
                    ProductCount = src.Products.Count,
                    CreatedAt = src.CreatedAt,
                    CreatedBy = src.CreatedBy,
                    LastModifiedAt = src.LastModifiedAt,
                    LastModifiedBy = src.LastModifiedBy,
                    // Initialize collections to empty
                    SubCategories = new List<CategoryDto>(),
                    // ParentCategory will be mapped separately
                    ParentCategory = null
                };
            });

        // Handle circular references in a post-processing step
        CreateMap<CategoryDto, CategoryDto>()
            .AfterMap((src, dest, context) => {
                // Handle circular references by creating new instances
                if (dest.ParentCategory != null)
                {
                    var parentCopy = new CategoryDto
                    {
                        Id = dest.ParentCategory.Id,
                        Name = dest.ParentCategory.Name,
                        Description = dest.ParentCategory.Description,
                        // Include necessary fields but don't continue the chain
                        SubCategories = new List<CategoryDto>(),
                        ParentCategory = null
                    };

                    // Replace the original reference
                    dest = dest with { ParentCategory = parentCopy };
                }
            });
    }
}