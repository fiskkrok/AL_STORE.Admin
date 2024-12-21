namespace Admin.Domain.Common.Exceptions;
public class ReservationNotFoundException : DomainException
{
    public ReservationNotFoundException(Guid orderId)
        : base($"Stock reservation not found for order {orderId}") { }
}
