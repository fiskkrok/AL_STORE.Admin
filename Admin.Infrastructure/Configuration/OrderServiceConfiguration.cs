using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Admin.Application.Common.Interfaces;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Admin.Infrastructure.Configuration;
public static class OrderServiceConfiguration
{
    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Add DbContext configuration
        services.AddScoped<DbContext>(provider => provider.GetService<AdminDbContext>());

        return services;
    }
}
