using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Orders.DTOs;
using AutoMapper;
using MediatR;

namespace Admin.Application.Orders.Queries;
public record GetOrderQuery(Guid Id) : IRequest<Result<OrderDto>>;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
            return Result<OrderDto>.Failure(new Error("Order.NotFound", "Order not found"));

        var dto = _mapper.Map<OrderDto>(order);
        return Result<OrderDto>.Success(dto);
    }
}