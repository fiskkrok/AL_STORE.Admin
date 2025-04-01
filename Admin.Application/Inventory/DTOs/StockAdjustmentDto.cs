using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Application.Inventory.DTOs;
public record StockAdjustmentDto
{
    public Guid ProductId { get; init; }
    public int Adjustment { get; init; }
    public string Reason { get; init; } = string.Empty;
}
