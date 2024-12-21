using Admin.Domain.Common;

namespace Admin.Domain.Events.Category;
public record CategorySortOrderChangedDomainEvent(Entities.Category Category, int NewSortOrder) : DomainEvent;
