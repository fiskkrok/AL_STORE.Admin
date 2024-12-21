using Admin.Domain.Enums;
using Ardalis.GuardClauses;

namespace Admin.Domain.ValueObjects;
public class Payment : ValueObject
{
    public string TransactionId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    private Payment() { }
    public Payment(
        string transactionId,
        PaymentMethod method,
        Money amount,
        PaymentStatus status,
        DateTime processedAt)
    {
        Guard.Against.NullOrWhiteSpace(transactionId, nameof(transactionId));
        Guard.Against.Null(amount, nameof(amount));

        TransactionId = transactionId;
        Method = method;
        Amount = amount;
        Status = status;
        ProcessedAt = processedAt;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TransactionId;
        yield return Method;
        yield return Amount;
        yield return Status;
        yield return ProcessedAt;
    }
}
