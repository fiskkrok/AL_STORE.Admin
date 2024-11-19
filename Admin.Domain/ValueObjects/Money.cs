using Ardalis.GuardClauses;

namespace Admin.Domain.ValueObjects;

public record Money
{
    private const int MaxDecimalPlaces = 2;

    private Money(decimal amount, string currency)
    {
        Amount = decimal.Round(amount, MaxDecimalPlaces, MidpointRounding.ToZero);
        Currency = currency;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public static Money From(decimal amount, string currency = "USD")
    {
        Guard.Against.Negative(amount, nameof(amount), "Amount cannot be negative");
        Guard.Against.NullOrWhiteSpace(currency, nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a three-letter ISO code", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "USD") => From(0, currency);

    public Money Add(Money other)
    {
        Guard.Against.Null(other, nameof(other));

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return From(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        Guard.Against.Null(other, nameof(other));

        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        return From(Amount - other.Amount, Currency);
    }

    public static implicit operator decimal(Money money) => money.Amount;
}