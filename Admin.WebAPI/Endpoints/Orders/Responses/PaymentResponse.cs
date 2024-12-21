using Admin.Application.Orders.DTOs;
using Admin.Domain.Enums;

namespace Admin.WebAPI.Endpoints.Orders.Responses;

public record PaymentResponse(PaymentDto Payment)
{
    public string TransactionId => Payment.TransactionId;
    public PaymentMethod Method => Payment.Method;
    public decimal Amount => Payment.Amount;
    public string Currency => Payment.Currency;
    public PaymentStatus Status => Payment.Status;
    public DateTime ProcessedAt => Payment.ProcessedAt;
}