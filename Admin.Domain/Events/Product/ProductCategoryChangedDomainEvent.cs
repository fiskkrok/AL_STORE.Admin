using Admin.Domain.Common;

namespace Admin.Domain.Events.Product;
public record ProductCategoryChangedDomainEvent(Entities.Product Product, Entities.Category OldCategory, Entities.Category NewCategory) : DomainEvent;

