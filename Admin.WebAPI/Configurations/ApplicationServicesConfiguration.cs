//using Admin.Application;
//using Admin.Application.Common.Interfaces;
//using Admin.Application.Products.Commands.CreateProduct;
//using Admin.Application.Products.Queries;
//using Admin.Infrastructure.Services;

//namespace Admin.WebAPI.Configurations;

///// <summary>
///// Application layer services configuration
///// </summary>
//public static class ApplicationServicesConfiguration
//{
//    /// <summary>
//    /// Adds application-layer services from the Admin.Application project
//    /// </summary>
//    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
//    {
//        // Add Application dependency injection
//        services.AddApplication();
//        // Register domain event service
//        services.AddScoped<IDomainEventService, DomainEventService>();
//        // Add MediatR handlers  
//        services.AddMediatR(cfg =>
//        {
//            cfg.RegisterServicesFromAssemblies(
//                typeof(Program).Assembly,                   // Web API Assembly
//                typeof(CreateProductCommand).Assembly,      // Application Assembly
//                typeof(GetProductsQueryHandler).Assembly    // Explicitly register the handlers assembly
//            );
//        });

//        // Add AutoMapper configuration
//        services.AddAutoMapper(cfg =>
//        {
//            cfg.AddMaps(
//                typeof(Program).Assembly,                   // Web API Assembly
//                typeof(CreateProductCommand).Assembly       // Application Assembly
//            );
//        });

//        return services;
//    }
//}