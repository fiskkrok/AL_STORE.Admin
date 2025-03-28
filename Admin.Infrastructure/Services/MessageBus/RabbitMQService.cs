using System.Text;

using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Events;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using RabbitMQ.Client;

namespace Admin.Infrastructure.Services.MessageBus;

public class RabbitMQService : IMessageBusService, IHostedService, IDisposable
{
    private IConnection _connection;
    private IChannel _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private bool _disposed;
    private readonly RabbitMQSettings _settings;

    public RabbitMQService(IOptions<RabbitMQSettings> options, ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        _settings = options.Value;

    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await InitializeAsync();
            _logger.LogInformation("RabbitMQ service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ service");
            // Consider whether to rethrow or not based on your app's requirements
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (_connection != null && _channel != null)
            {
                return; // Already initialized
            }

            _logger.LogInformation("Initializing RabbitMQ connection to host {Host}", _settings.Host);

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                UserName = _settings.Username,
                Password = _settings.Password,
                // Add these for more reliability
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            // Connection attempts
            int retryCount = 0;
            const int maxRetries = 5;

            while (retryCount < maxRetries)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                    break;
                }
                catch (Exception ex) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/{MaxAttempts}), retrying in {Seconds}s...",
                        retryCount, maxRetries, Math.Pow(2, retryCount));
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
            }

            if (_connection == null || _channel == null)
            {
                throw new InvalidOperationException("Failed to establish RabbitMQ connection after multiple attempts");
            }


            // Declare exchanges for different message types
            await _channel.ExchangeDeclareAsync("product-events", ExchangeType.Topic, durable: true);
            await _channel.ExchangeDeclareAsync("image-processing", ExchangeType.Direct, durable: true);

            // Declare queues
            await _channel.QueueDeclareAsync("product-updates", durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueDeclareAsync("stock-updates", durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueDeclareAsync("image-processing", durable: true, exclusive: false, autoDelete: false);

            // Bind queues to exchanges
            await _channel.QueueBindAsync("product-updates", "product-events", "product.*");
            await _channel.QueueBindAsync("stock-updates", "product-events", "stock.*");
            await _channel.QueueBindAsync("image-processing", "image-processing", "image.process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQService));
        }

        // Retry loop for message publishing
        int retries = 0;
        const int maxRetries = 3;

        while (true)
        {
            try
            {
                if (_channel == null)
                {
                    await InitializeAsync();
                    if (_channel == null)
                    {
                        throw new InvalidOperationException("Failed to initialize RabbitMQ channel");
                    }
                }

                var messageType = message.GetType();
                var exchangeName = GetExchangeName(messageType);
                var routingKey = GetRoutingKey(messageType);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                    Type = messageType.Name
                };

                await _channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published message of type {MessageType} with ID {MessageId}",
                    messageType.Name, properties.MessageId);

                return; // Success, exit the method
            }
            catch (Exception ex)
            {
                retries++;

                if (retries >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to publish message of type {MessageType} after {Retries} retries",
                        typeof(T).Name, retries);
                    throw;
                }

                _logger.LogWarning(ex, "Error publishing message (attempt {Attempt}/{MaxRetries}), retrying...",
                    retries, maxRetries);

                // Clean up and try to re-establish connection
                try
                {
                    _channel?.Dispose();
                    _connection?.Dispose();
                    _channel = null;
                    _connection = null;
                }
                catch { /* Ignore cleanup errors */ }

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retries))); // Exponential backoff
            }
        }
    }

    private static string GetExchangeName(Type messageType)
    {
        return messageType == typeof(ImageProcessedIntegrationEvent) ? "image-processing" : "product-events";
    }

    private string GetRoutingKey(Type messageType)
    {
        return messageType.Name switch
        {
            nameof(ProductCreatedIntegrationEvent) => "product.created",
            nameof(ProductUpdatedIntegrationEvent) => "product.updated",
            nameof(ProductDeletedIntegrationEvent) => "product.deleted",
            nameof(ProductStockUpdatedIntegrationEvent) => "stock.updated",
            nameof(ImageProcessedIntegrationEvent) => "image.process",
            _ => throw new ArgumentException($"Unknown message type: {messageType.Name}")
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        _disposed = true;
    }
}

