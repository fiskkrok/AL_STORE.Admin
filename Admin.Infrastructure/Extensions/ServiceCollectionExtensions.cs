using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Settings;
using Admin.Application.Services;
using Admin.Infrastructure.Persistence.Repositories;
using Admin.Infrastructure.Services;
using Admin.Infrastructure.Services.Decorators;
using Admin.Infrastructure.Services.FileStorage;
using Admin.Infrastructure.Services.MessageBus;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Admin.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, bool enableLogging = true)
    {
        // IMPORTANT: Register the domain event mapper first
        services.AddSingleton<IDomainEventMapper, DomainEventMapper>();
        // Then register domain event service which depends on the mapper
        services.AddScoped<IDomainEventService>(sp =>
        {
            var service = new DomainEventService(
                sp.GetRequiredService<IPublisher>(),
                sp.GetRequiredService<IMessageBusService>(),
                sp.GetRequiredService<IDomainEventMapper>());

            //if (enableLogging)
            //{
            //    return new LoggingDomainEventServiceDecorator(
            //        service,
            //        sp.GetRequiredService<ILogger<LoggingDomainEventServiceDecorator>>());
            //}

            return service;
        });


        // Register domain services with optional logging decorators
        services.AddScoped<IUserService>(sp =>
        {
            var service = new UserService(
                sp.GetRequiredService<IUserRepository>(),
                sp.GetRequiredService<ITokenService>(),
                sp.GetRequiredService<IUnitOfWork>());

            if (enableLogging)
            {
                return new LoggingUserServiceDecorator(
                    service,
                    sp.GetRequiredService<ILogger<LoggingUserServiceDecorator>>());
            }

            return service;
        });

        services.AddScoped<ITokenService>(sp =>
        {
            var service = new TokenService(
                sp.GetRequiredService<IOptions<JwtSettings>>(),
                sp.GetRequiredService<IDateTimeProvider>(),
                sp.GetRequiredService<ICacheService>());

            //if (enableLogging)
            //{
            //    return new LoggingTokenServiceDecorator(
            //        service, 
            //        sp.GetRequiredService<ILogger<LoggingTokenServiceDecorator>>());
            //}

            return service;
        });

        // Other services that don't need decorators
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Core infrastructure services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Configure and register cache service
        services.Configure<CacheSettings>(configuration.GetSection("Cache"));

        // Register message bus
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));
        services.AddSingleton<IMessageBusService, RabbitMQService>();
        services.AddHostedService(sp => (RabbitMQService)sp.GetRequiredService<IMessageBusService>());

        // Register file storage
        services.Configure<AzureBlobStorageSettings>(configuration.GetSection("AzureBlobStorage"));
        services.AddScoped<IFileStorage, AzureBlobStorageService>();
        services.AddHostedService<BlobContainerInitializer>();

        return services;
    }
}