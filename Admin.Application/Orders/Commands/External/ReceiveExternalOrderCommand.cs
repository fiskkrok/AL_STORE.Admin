//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Admin.Application.Common.Interfaces;
//using Admin.Application.Common.Models;
//using Admin.Application.Orders.DTOs;
//using Admin.Application.Orders.DTOs.External;
//using Admin.Domain.Entities;
//using Admin.Domain.ValueObjects;

//using MediatR;

//namespace Admin.Application.Orders.Commands.External;

//public class ReceiveExternalOrderCommand : IRequest<Result<Guid>>
//{
//    public Guid RequestId { get; set; }
//    public IEnumerable<ExternalOrderItemDto> Items { get; set; }
//    public Guid CustomerId { get; set; }
//    public ShippingInfoDto ShippingAddress { get; set; }
//    public ShippingInfoDto BillingAddress { get; set; }
//    public string? Notes { get; set; }
//    public string Source { get; set; }
//    public string Currency { get; set; }
//    public Guid ExternalId { get; set; }
//    public Guid ExternalOrderNumber { get; set; }
//}
//public class ReceiveExternalOrderCommandHandler : IRequestHandler<ReceiveExternalOrderCommand, Result<Guid>>
//{
//    private readonly IOrderRepository _orderRepository;
//    private readonly IIdempotencyService _idempotencyService;
//    private readonly IStockRepository _stockRepository;
//    private readonly IUnitOfWork _unitOfWork;

//    public ReceiveExternalOrderCommandHandler(IUnitOfWork unitOfWork, IStockRepository stockRepository, IIdempotencyService idempotencyService, IOrderRepository orderRepository)
//    {
//        _unitOfWork = unitOfWork;
//        _stockRepository = stockRepository;
//        _idempotencyService = idempotencyService;
//        _orderRepository = orderRepository;
//    }

//    public async Task<Result<Guid>> Handle(ReceiveExternalOrderCommand command, CancellationToken cancellationToken)
//    {
//        // Check for duplicate request
//        if (await _idempotencyService.IsDuplicateRequestAsync(command.RequestId))
//        {
//            var existingOrderId = await _idempotencyService.GetExistingResultAsync<Guid>(command.RequestId);
//            return Result<Guid>.Success(existingOrderId);
//        }

//        // Begin transaction
//        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

//        try
//        {
//            // Check stock availability
//            var stockAvailable = await _stockRepository.CheckStockAvailabilityAsync(
//                command.Items.ToDictionary(i => i.ProductId, i => i.Quantity),
//                cancellationToken);

//            if (stockAvailable.Values.Any(v => !v))
//            {
//                return Result<Guid>.Failure(new Error("Order.InsufficientStock", "One or more items are out of stock"));
//            }

//            // Create the order
//            var order = new Order(
//                command.CustomerId,
//                command.ShippingAddress.ToEntity(),
//                command.BillingAddress.ToEntity(),
//                command.Notes);

//            // Add external reference
//            order.AddExternalReference(command.Source, command.ExternalId, command.ExternalOrderNumber);

//            // Add items
//            foreach (var item in command.Items)
//            {
//                // Correcting the syntax error in the AddItem method call
//                order.AddItem(
//                   item.ProductId,
//                   item.VariantId,
//                   item.Quantity,
//                   Money.From(item.UnitPrice, command.Currency));
                
//            }

//            // Reserve stock
//            foreach (var item in command.Items)
//            {
//                await _stockRepository.ReserveStockAsync(
//                    item.ProductId, item.VariantId, item.Quantity, order.Id, cancellationToken);
//            }

//            await _orderRepository.AddAsync(order, cancellationToken);
//            await _unitOfWork.SaveChangesAsync(cancellationToken);
//            await transaction.CommitAsync(cancellationToken);

//            // Store result for idempotency
//            await _idempotencyService.SaveResultAsync(command.RequestId, order.Id);

//            return Result<Guid>.Success(order.Id);
//        }
//        catch (Exception ex)
//        {
//            await transaction.RollbackAsync(cancellationToken);
//            return Result<Guid>.Failure(new Error("Order.CreateFailed", ex.Message));
//        }
//    }
//}