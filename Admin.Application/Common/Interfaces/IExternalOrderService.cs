using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Common.Models;
using Admin.Application.Orders.DTOs.External;
using Admin.Domain.Enums;

namespace Admin.Application.Common.Interfaces;
public interface IExternalOrderService
{
    Task<Result<Guid>> ReceiveOrderAsync(ExternalOrderDto order);
    Task<Result<bool>> NotifyStatusChangeAsync(Guid orderId, OrderStatus newStatus);
    Task<Result<bool>> ValidateOrderHashAsync(string payload, string signature, string secret);
}
