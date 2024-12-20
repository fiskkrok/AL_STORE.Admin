using Admin.Domain.Common;

using MediatR;

namespace Admin.Application.Common.Models;

public class DomainEventNotification<TEvent> : INotification where TEvent : DomainEvent
{
    public TEvent DomainEvent { get; }

    public DomainEventNotification(TEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
