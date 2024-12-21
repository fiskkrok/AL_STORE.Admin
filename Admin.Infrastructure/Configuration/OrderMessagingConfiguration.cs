using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Admin.Application.Orders.Events;
using FluentValidation;
using MassTransit;

namespace Admin.Infrastructure.Configuration;
public static class OrderMessagingConfiguration
{
    public static void ConfigureOrderMessaging(this IRabbitMqBusFactoryConfigurator cfg, IBusRegistrationContext context)
    {
        cfg.Message<OrderCreatedIntegrationEvent>(e => e.SetEntityName("orders.created"));
        cfg.Message<OrderStatusChangedIntegrationEvent>(e => e.SetEntityName("orders.status-changed"));
        cfg.Message<OrderPaymentAddedIntegrationEvent>(e => e.SetEntityName("orders.payment-added"));
        cfg.Message<OrderShippingUpdatedIntegrationEvent>(e => e.SetEntityName("orders.shipping-updated"));
        cfg.Message<OrderCancelledIntegrationEvent>(e => e.SetEntityName("orders.cancelled"));

        // Configure error handling and retry policies
        cfg.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            r.Ignore<ValidationException>();
        });
    }
}
