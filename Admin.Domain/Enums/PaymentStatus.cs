using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin.Domain.Enums;
public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded
}
