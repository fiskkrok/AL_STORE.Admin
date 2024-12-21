using Admin.Application.Orders.DTOs;
using Admin.Domain.Entities;
using Admin.Domain.ValueObjects;

using AutoMapper;

namespace Admin.Application.Mappings;
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount))
            .ForMember(dest => dest.ShippingCost, opt => opt.MapFrom(src => src.ShippingCost.Amount))
            .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.Tax.Amount))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Total.Currency));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount));

        CreateMap<Address, AddressDto>();
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency));
        CreateMap<ShippingInfo, ShippingInfoDto>();
    }
}
