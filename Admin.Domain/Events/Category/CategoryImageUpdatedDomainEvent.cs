using Admin.Domain.Common;

namespace Admin.Domain.Events.Category;
public record CategoryImageUpdatedDomainEvent(Entities.Category Category, string? ImageUrl) : DomainEvent;