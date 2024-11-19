using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence.Repositories;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Services.FileStorage;
using Admin.Infrastructure.Services.MessageBus;
using Admin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AdminDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName)));

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdminDbContext>());

        // Register services
        services.AddScoped<IDomainEventService, DomainEventService>();

        // Configure RabbitMQ
        services.Configure<RabbitMQSettings>(
            configuration.GetSection("RabbitMQ"));
        services.AddScoped<IMessageBusService, RabbitMQService>();

        // Configure Azure Blob Storage
        services.Configure<AzureBlobStorageSettings>(
            configuration.GetSection("AzureBlobStorage"));
        services.AddScoped<IFileStorage, AzureBlobStorageService>();

        return services;
    }
}