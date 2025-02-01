using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Models;
using Admin.Domain.ValueObjects;
using MediatR;

namespace Admin.Application.Orders.Commands;
// Admin.Application/Orders/Commands
public record AddOrderItemCommand : IRequest<Result<Unit>>
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public Guid? VariantId { get; init; }
    public int Quantity { get; init; }
    public required Money UnitPrice { get; init; }
}
public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, Result<Unit>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockRepository _stockRepository;

    public AddOrderItemCommandHandler(IOrderRepository orderRepository, IProductRepository productRepository, IStockRepository stockRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _stockRepository = stockRepository;
    }

    public async Task<Result<Unit>> Handle(AddOrderItemCommand command, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        var variant = command.VariantId.HasValue
            ? await _productRepository.GetVariantByIdAsync(command.VariantId.Value, ct)
            : null;

        // Check stock availability
        await _stockRepository.ReserveStockAsync(
            command.ProductId,
            command.VariantId,
            command.Quantity,
            order.Id,
            ct);

        // Add item to order
        order.AddVariantItem(product, variant, command.Quantity, command.UnitPrice);

        await _orderRepository.UpdateAsync(order, ct);
        return Result<Unit>.Success(Unit.Value);
    }
}
