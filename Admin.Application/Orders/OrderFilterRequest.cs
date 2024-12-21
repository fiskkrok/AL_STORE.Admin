using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Enums;

namespace Admin.Application.Orders;
public class OrderFilterRequest
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
