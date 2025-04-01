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
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Total.Currency))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Status.ToString().ToLowerInvariant() : ""))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Method.ToString() : ""))
            .ConstructUsing((src, context) => new OrderDto
            {
                Id = src.Id,
                OrderNumber = src.OrderNumber,
                CustomerId = src.CustomerId,
                Status = src.Status.ToString().ToLowerInvariant(),
                Subtotal = src.Subtotal.Amount,
                ShippingCost = src.ShippingCost.Amount,
                Tax = src.Tax.Amount,
                Total = src.Total.Amount,
                Currency = src.Total.Currency,
                Notes = src.Notes,
                CancelledAt = src.CancelledAt,
                CancellationReason = src.CancellationReason,
                CreatedAt = src.CreatedAt,
                CreatedBy = src.CreatedBy,
                LastModifiedAt = src.LastModifiedAt,
                LastModifiedBy = src.LastModifiedBy,
                Items = new List<OrderItemDto>(),
                // ShippingAddress and BillingAddress will be mapped separately
                // Payment and ShippingInfo will be mapped separately
            });

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount));

        CreateMap<Address, AddressDto>();

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Amount.Currency))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()))
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.ToString()));

        CreateMap<ShippingInfo, ShippingInfoDto>();
    }
}
