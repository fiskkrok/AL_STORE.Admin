using Admin.Domain.Common;
using MediatR;

namespace Admin.Application.Common.Interfaces;
public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
