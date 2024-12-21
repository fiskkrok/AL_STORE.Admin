using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.Common.Exceptions;
public class ReservationNotFoundException : DomainException
{
    public ReservationNotFoundException(Guid orderId)
        : base($"Stock reservation not found for order {orderId}") { }
}
