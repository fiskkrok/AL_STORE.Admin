using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence.Repositories;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Services.FileStorage;
using Admin.Infrastructure.Services.MessageBus;
using Admin.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.Extensions.Options;
using Admin.Application.Common.Behaviors;
using Admin.Infrastructure.Configuration;
using FluentValidation;
using MediatR;
using RabbitMQ.Client;

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
        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AdminDbContext>());
        services.Decorate<IProductRepository, CachedProductRepository>();
        services.AddErrorHandling();
        // Register services
        services.AddScoped<IDomainEventService, DomainEventService>();

        // Configure RabbitMQ
        services.Configure<RabbitMQSettings>(
            configuration.GetSection("RabbitMQ"));
        services.AddScoped<IMessageBusService, RabbitMQService>();
        // Configure Azure Blob Storage
        services.Configure<AzureBlobStorageSettings>(
            configuration.GetSection("AzureBlobStorageSettings"));
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureBlobStorageSettings>>().Value;

            if (settings.ConnectionString == "UseDevelopmentStorage=true")
            {
                // Local development using Azurite
                return new BlobServiceClient(
                    new Uri("http://127.0.0.1:10000/devstoreaccount1"),
                    new StorageSharedKeyCredential(
                        "devstoreaccount1",
                        "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
            }

            // Production/staging environment
            return new BlobServiceClient(settings.ConnectionString);
        });
        services.AddScoped<IFileStorage, AzureBlobStorageService>();
        // Ensure the blob container exists
        services.AddHostedService<BlobContainerInitializer>();
        return services;
    }


}