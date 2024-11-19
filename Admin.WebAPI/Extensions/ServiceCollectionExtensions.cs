using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Services.MessageBus;

namespace Admin.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductServices(this IServiceCollection services, IConfiguration configuration)
    {

        // Register AutoMapper
        services.AddAutoMapper(typeof(Program).Assembly);

        // Configure RabbitMQ Settings
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

        // Register Services
        //services.AddScoped<IProductService, ProductService>();
        services.AddSingleton<IMessageBusService, RabbitMQService>();

        // Add any other required services here
        // services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        return services;
    }
}