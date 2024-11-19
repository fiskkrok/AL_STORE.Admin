using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;

public record CategoryCreatedDomainEvent(Category Category) : DomainEvent;