using Admin.Domain.Common;

namespace Admin.Application.Common.Interfaces;
public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
