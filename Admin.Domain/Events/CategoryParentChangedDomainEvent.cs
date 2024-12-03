using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;
public record CategoryParentChangedDomainEvent(Category Category, Category? OldParent, Category? NewParent) : DomainEvent;


