// Admin.Application/DependencyInjection.cs

using System.Reflection;
using Admin.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<StockManagementService>();
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}