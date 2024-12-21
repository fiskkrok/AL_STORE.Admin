using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Category;
public record CategoryParentChangedDomainEvent(Entities.Category Category, Entities.Category? OldParent, Entities.Category? NewParent) : DomainEvent;


