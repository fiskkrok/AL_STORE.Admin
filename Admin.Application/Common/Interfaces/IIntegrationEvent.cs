namespace Admin.Application.Common.Interfaces;
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
}
