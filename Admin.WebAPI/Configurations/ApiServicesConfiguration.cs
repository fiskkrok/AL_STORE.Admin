using System.Reflection;
using Microsoft.OpenApi.Models;
using FastEndpoints;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// API services configuration
/// </summary>
public static class ApiServicesConfiguration
{
    /// <summary>
    /// Adds API-related services, including FastEndpoints and Swagger
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure FastEndpoints
        services.AddFastEndpoints()
            .AddOpenApi();

        // Configure CORS
        AddCors(services, configuration);

        // Configure Swagger/OpenAPI
        AddSwagger(services);

        return services;
    }

    /// <summary>
    /// Adds CORS configuration
    /// </summary>
    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        // Get allowed origins from configuration
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            });
        });
    }

    /// <summary>
    /// Adds Swagger/OpenAPI configuration
    /// </summary>
    private static void AddSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Admin API",
                Version = "v1",
                Description = "API for managing admin operations",
                Contact = new OpenApiContact
                {
                    Name = "Your Name",
                    Email = "your.email@example.com",
                    Url = new Uri("https://yourwebsite.com")
                },
                License = new OpenApiLicense
                {
                    Name = "Use under License",
                    Url = new Uri("https://example.com/license")
                }
            });

            // Configure JWT authentication in Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Configure schema ID generation for generic types
            c.CustomSchemaIds(type =>
            {
                var name = type.Name;
                if (type.IsGenericType)
                {
                    var genericArgs = string.Join("", type.GetGenericArguments().Select(t => t.Name));
                    name = $"{type.Name.Split('`')[0]}{genericArgs}";
                }
                return name;
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
    }

    /// <summary>
    /// Configures API endpoints
    /// </summary>
    public static IApplicationBuilder UseApiEndpoints(this IApplicationBuilder app)
    {
        // Configure FastEndpoints
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.RoutePrefix = "api";
        });

        return app;
    }
    public static IEndpointRouteBuilder UseHealthEndpoints(this IEndpointRouteBuilder app)
    {
        

        // Health check endpoint
        app.Map("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithTags("Health")
            .WithOpenApi();

        return app;
    }
}