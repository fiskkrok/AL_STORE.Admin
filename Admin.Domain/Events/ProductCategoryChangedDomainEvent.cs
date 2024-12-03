using Admin.Domain.Common;
using Admin.Domain.Entities;

namespace Admin.Domain.Events;
public record ProductCategoryChangedDomainEvent(Product Product, Category OldCategory, Category NewCategory) : DomainEvent;

