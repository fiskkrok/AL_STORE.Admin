using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Application.Orders.DTOs;
using Admin.Domain.Enums;
using AutoMapper;
using MediatR;

namespace Admin.Application.Orders.Queries;
public record GetOrdersQuery : IRequest<Result<PagedList<OrderDto>>>
{
    public Guid? CustomerId { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public decimal? MinTotal { get; init; }
    public decimal? MaxTotal { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PagedList<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var filter = new OrderFilterRequest
            {
                CustomerId = request.CustomerId,
                Status = request.Status,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                MinTotal = request.MinTotal,
                MaxTotal = request.MaxTotal,
                SearchTerm = request.SearchTerm,
                SortBy = request.SortBy,
                SortDescending = request.SortDescending,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var (orders, totalCount) = await _orderRepository.GetOrdersAsync(filter, cancellationToken);

            var dtos = orders.Select(o => _mapper.Map<OrderDto>(o)).ToList();
            var pagedList = new PagedList<OrderDto>(dtos, totalCount, request.PageNumber, request.PageSize);

            return Result<PagedList<OrderDto>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            return Result<PagedList<OrderDto>>.Failure(new Error("Orders.GetFailed", ex.Message));
        }
    }
}
