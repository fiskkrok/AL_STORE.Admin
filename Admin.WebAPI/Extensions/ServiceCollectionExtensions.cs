using System.Reflection;

using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Commands.CreateProduct;
using Admin.Application.Products.Queries;
using Admin.Infrastructure.Persistence;
using Admin.Infrastructure.Persistence.Seeder;
using Admin.WebAPI.Infrastructure;
using Admin.WebAPI.Services;

using FastEndpoints;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Admin.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAPI(this IServiceCollection services)
    {
        // Add logging services
        services.AddLogging();

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "https://localhost:5001";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = new[] { "api" }, // Add both valid audiences
                    ValidateIssuer = true,
                    ValidIssuer = "https://localhost:5001",
                    ValidateLifetime = true
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>(); // Added
                        logger.LogError(context.Exception, "Authentication failed"); // Modified
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>(); // Added
                        logger.LogInformation("Token validated successfully"); // Modified
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>(); // Added
                        logger.LogWarning("OnChallenge: {Error}", context.Error); // Modified
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>(); // Added
                        logger.LogInformation("Token received: {Token}", context.Token); // Modified
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
        services.AddAuthorizationBuilder()
            .AddPolicy("FullAdminAccess", policy =>
                policy.RequireRole("SystemAdministrator")
                    .RequireClaim("scope", "api.full"))
            .AddPolicy("ProductEdit", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c is { Type: "scope", Value: "products.update" or "api.full" } ||
                        context.User.IsInRole("SystemAdministrator")
                    )))
            .AddPolicy("ProductsCreate", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c is { Type: "scope", Value: "products.create" or "api.full" } ||
                        context.User.IsInRole("SystemAdministrator")
                    )))
            .AddPolicy("ProductsRead", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c is { Type: "scope", Value: "products.read" or "api.full" } ||
                        context.User.IsInRole("SystemAdministrator")
                    )))
            .AddPolicy("ProductsDelete", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c is { Type: "scope", Value: "products.delete" or "api.full" } ||
                        context.User.IsInRole("SystemAdministrator")
                    )));
        // Add FastEndpoints

        services.AddFastEndpoints()
            .AddOpenApi();
        // Configure CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>  // Note: AddDefaultPolicy instead of AddPolicy
            {
                builder
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition"); // Add if you need to expose headers
            });
        });

        // Configure Swagger/OpenAPI
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

        // Add application services
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(Program).Assembly,                   // Web API Assembly
                typeof(CreateProductCommand).Assembly,      // Application Assembly
                typeof(GetProductsQueryHandler).Assembly    // Explicitly register the handlers assembly
            );
        });

        // Also ensure AutoMapper is properly configured
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(
                typeof(Program).Assembly,                   // Web API Assembly
                typeof(CreateProductCommand).Assembly       // Application Assembly
            );
        });
       
        services.AddScoped<ICategorySeeder, CategoryDbSeeder>();
        services.AddScoped<IProductSeeder, ProductDbSeeder>();
        services.AddScoped<IDbSeeder, MainDbSeeder>();
        // Register AutoMapper


        return services;
    }

    public static async Task<WebApplication> AddPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Admin API V1");
                c.RoutePrefix = "api-docs";
                // Add Swagger UI customization
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
                // Customize the UI
                c.InjectStylesheet("/swagger-ui/custom.css");
                c.EnableFilter();
                c.DisplayRequestDuration();

            });
        }

        // Add logging middleware
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // Added
            logger.LogInformation("Handling request: {Method} {Path}", context.Request.Method, context.Request.Path); // Added
            await next.Invoke();
            logger.LogInformation("Finished handling request."); // Added
        });

        app.UseCors();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        // Configure FastEndpoints
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.RoutePrefix = "api";
            // TODO: Add proper versioning middleware
            //c.Versioning.Prefix = "v";
            //c.Versioning.DefaultVersion = 1;
        });

        // Health check endpoint
        app.MapGet("/health", [Authorize(Policy = "FullAdminAccess")] () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithTags("Health")
            .WithOpenApi();

        // SignalR hubs
        app.MapSignalRHubs();

        // Seed the database
        try
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var seeder = services.GetRequiredService<IDbSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }

        return app;
    }

        public static IServiceCollection AddCustomHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var rabbitHost = configuration["RabbitMQ:Host"];
            var rabbitUsername = configuration["RabbitMQ:Username"];
            var rabbitPassword = configuration["RabbitMQ:Password"];
            var rabbitVirtualHost = configuration["RabbitMQ:VirtualHost"];

            var rabbitConnectionString = $"amqp://{rabbitUsername}:{rabbitPassword}@{rabbitHost}/{rabbitVirtualHost}";

            if (string.IsNullOrEmpty(rabbitConnectionString))
            {
                throw new ArgumentNullException(nameof(rabbitConnectionString), "RabbitMQ connection string is not configured.");
            }
        services.AddHealthChecks()
                .AddCheck<RedisHealthCheck>(
                    "redis_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "cache", "redis" })
                .AddDbContextCheck<AdminDbContext>(
                    "database_health_check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database", "sql" })
                .AddRabbitMQ(
                    rabbitConnectionString: rabbitConnectionString!,
                    name: "rabbitmq_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "messaging", "rabbitmq" });

            return services;
        }
    }
   