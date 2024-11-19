using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;

public record CategoryDeletedDomainEvent(Category Category) : DomainEvent;