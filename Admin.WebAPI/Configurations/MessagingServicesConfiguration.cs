using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Services.MessageBus;
using Admin.WebAPI.Hubs;
using Admin.WebAPI.Messaging.Consumers;

using FluentValidation;

using MassTransit;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Messaging services configuration
/// </summary>
public static class MessagingServicesConfiguration
{
    /// <summary>
    /// Adds messaging services, including RabbitMQ and SignalR
    /// </summary>
    public static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure SignalR with Redis backplane
        AddSignalR(services, configuration);

        // Configure RabbitMQ
        AddRabbitMQ(services, configuration);

        return services;
    }

    /// <summary>
    /// Adds SignalR with Redis backplane for scaling
    /// </summary>
    private static void AddSignalR(IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100 KB
            options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(20);
        })
            .AddStackExchangeRedis(redisConnection!, options =>
            {
                options.Configuration.ChannelPrefix = "Admin_";
            });
    }

    /// <summary>
    /// Adds RabbitMQ and MassTransit for messaging
    /// </summary>
    private static void AddRabbitMQ(IServiceCollection services, IConfiguration configuration)
    {
        // Configure RabbitMQ Settings
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

        // Add RabbitMQ service
        services.AddSingleton<RabbitMQService>();
        services.AddSingleton<IMessageBusService>(sp => sp.GetRequiredService<RabbitMQService>());
        services.AddHostedService(sp => sp.GetRequiredService<RabbitMQService>());

        // Configure MassTransit with RabbitMQ
        services.AddMassTransit(x =>
        {
            // Register consumers
            RegisterConsumers(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
                                     ?? throw new InvalidOperationException("RabbitMQ settings are missing");

                cfg.Host(rabbitMqSettings.Host, h =>
                {
                    h.Username(rabbitMqSettings.Username);
                    h.Password(rabbitMqSettings.Password);
                });

                // Configure error handling and retry policies
                cfg.UseMessageRetry(r =>
                {
                    r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
                    r.Ignore<ValidationException>();
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    /// <summary>
    /// Registers MassTransit message consumers
    /// </summary>
    private static void RegisterConsumers(IBusRegistrationConfigurator configurator)
    {
        // Product-related consumers
        configurator.AddConsumer<ProductCreatedConsumer>();
        configurator.AddConsumer<StockUpdatedConsumer>();
        configurator.AddConsumer<VariantCreatedConsumer>();
        configurator.AddConsumer<VariantStockUpdatedConsumer>();
        configurator.AddConsumer<ImageProcessingConsumer>();

        // Order-related consumers
        configurator.AddConsumer<OrderStatusChangedConsumer>();
        configurator.AddConsumer<OrderPaymentAddedConsumer>();
        configurator.AddConsumer<OrderShippingUpdatedConsumer>();
        configurator.AddConsumer<OrderCancelledConsumer>();
    }

    /// <summary>
    /// Maps SignalR hubs to endpoints
    /// </summary>
    public static IEndpointRouteBuilder MapSignalRHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<ProductHub>("/hubs/products");
        endpoints.MapHub<OrderHub>("/hubs/orders");
        endpoints.MapHub<CategoryHub>("/hubs/categories");
        endpoints.MapHub<ProductVariantHub>("/hubs/productvariant");

        return endpoints;
    }
}