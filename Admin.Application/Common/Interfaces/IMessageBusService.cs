namespace Admin.Application.Common.Interfaces;

public interface IMessageBusService
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IMessage;
}
