using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Category;
public record CategorySortOrderChangedDomainEvent(Entities.Category Category, int NewSortOrder) : DomainEvent;
