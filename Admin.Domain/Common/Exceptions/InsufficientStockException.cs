using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.Common.Exceptions;
public class InsufficientStockException : DomainException
{
    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}") { }
}
