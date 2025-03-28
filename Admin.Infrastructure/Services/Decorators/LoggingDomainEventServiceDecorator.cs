using Admin.Application.Common.Interfaces;
using Admin.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Admin.Infrastructure.Services.Decorators
{
    public class LoggingDomainEventServiceDecorator : IDomainEventService
    {
        private readonly IDomainEventService _inner;
        private readonly ILogger<LoggingDomainEventServiceDecorator> _logger;

        public LoggingDomainEventServiceDecorator(IDomainEventService inner, ILogger<LoggingDomainEventServiceDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Publishing domain event: {EventType}", domainEvent.GetType().Name);
            await _inner.PublishAsync(domainEvent, cancellationToken);
            _logger.LogInformation("Published domain event: {EventType}", domainEvent.GetType().Name);
        }
    }
}
