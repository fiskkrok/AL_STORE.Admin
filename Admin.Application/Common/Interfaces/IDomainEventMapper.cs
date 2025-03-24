using Admin.Domain.Common;

namespace Admin.Application.Common.Interfaces;
public interface IDomainEventMapper
{
    IMessage? MapToIntegrationEvent(DomainEvent domainEvent);
}
