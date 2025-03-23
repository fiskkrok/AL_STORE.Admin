using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure.Extensions;
public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, bool enableCaching = true)
    {
        // Register the repository factory
        services.AddScoped<RepositoryFactory>();

        // Register repositories
        services.AddScoped<IProductRepository>(sp =>
            sp.GetRequiredService<RepositoryFactory>().CreateProductRepository(enableCaching));

        services.AddScoped<ICategoryRepository>(sp =>
            sp.GetRequiredService<RepositoryFactory>().CreateCategoryRepository(enableCaching));

        services.AddScoped<IOrderRepository>(sp =>
            sp.GetRequiredService<RepositoryFactory>().CreateOrderRepository(enableCaching));

        services.AddScoped<IStockRepository>(sp =>
            sp.GetRequiredService<RepositoryFactory>().CreateStockRepository(enableCaching));

        services.AddScoped<IUserRepository>(sp =>
            sp.GetRequiredService<RepositoryFactory>().CreateUserRepository(enableCaching));

        return services;
    }
}
