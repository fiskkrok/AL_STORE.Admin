using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence.Repositories;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Services.MessageBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Admin.Infrastructure.Configuration;
using Admin.Infrastructure.Services;

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
                b =>
                {
                    b.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure();
                    // Add this to see what's happening
                    b.CommandTimeout(30);
                }));
        services.AddEventHandling();
        services.AddOrderServices();
        services.AddFileHandelingConfiguration( configuration);
        services.AddErrorHandling();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdminDbContext>());
        services.Decorate<IProductRepository, CachedProductRepository>();

        // Configure RabbitMQ
        services.Configure<RabbitMQSettings>(
            configuration.GetSection("RabbitMQ"));
        services.AddScoped<IMessageBusService, RabbitMQService>();
        // Configure Azure Blob Storage
        return services;
    }

    
}