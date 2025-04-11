using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Admin.Application.Products.DTOs;
using Admin.Domain.Entities;

using AutoMapper;

namespace Admin.Application.Mappings;
public class ProductTypeMappingProfile : Profile
{
    public ProductTypeMappingProfile()
    {
        CreateMap<ProductType, ProductTypeDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Attributes, opt => opt.Ignore())
            .AfterMap((src, dest) => {
                dest.Attributes = JsonSerializer.Deserialize<List<ProductTypeAttributeDto>>(src.AttributesJson) ?? new List<ProductTypeAttributeDto>();
            });
    }
}
