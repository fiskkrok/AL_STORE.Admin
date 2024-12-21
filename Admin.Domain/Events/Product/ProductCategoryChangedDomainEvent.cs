using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events.Product;
public record ProductCategoryChangedDomainEvent(Entities.Product Product, Entities.Category OldCategory, Entities.Category NewCategory) : DomainEvent;

