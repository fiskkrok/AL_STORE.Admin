using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Common;

namespace Admin.Domain.Events.Order;
public record OrderCreatedDomainEvent(Entities.Order Order) : DomainEvent;