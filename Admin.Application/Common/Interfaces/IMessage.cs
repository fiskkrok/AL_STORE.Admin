namespace Admin.Application.Common.Interfaces;

public interface IMessage
{
    Guid EventId { get; }
    DateTime Timestamp { get; }

}
