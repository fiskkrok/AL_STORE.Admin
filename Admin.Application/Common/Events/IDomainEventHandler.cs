using Admin.Application.Common.Models;
using Admin.Domain.Common;

using MediatR;

namespace Admin.Application.Common.Events;

public interface IDomainEventHandler<TEvent> : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : DomainEvent
{
}