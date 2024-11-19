using Admin.Application;
using Admin.Application.Common.Interfaces;
using Admin.Application.Products.Commands.CreateProduct;
using Admin.Application.Products.Queries;
using Admin.Infrastructure;
using Admin.Infrastructure.Persistence.Seeder;
using Admin.WebAPI.Infrastructure;
using Admin.WebAPI.Services;

using FastEndpoints;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.OpenApi.Models;

using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

// Configure Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure JWT Bearer settings
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
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
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,                   // Web API Assembly
        typeof(CreateProductCommand).Assembly,      // Application Assembly
        typeof(GetProductsQueryHandler).Assembly    // Explicitly register the handlers assembly
    );
});

// Also ensure AutoMapper is properly configured
builder.Services.AddAutoMapper(cfg => {
    cfg.AddMaps(
        typeof(Program).Assembly,                   // Web API Assembly
        typeof(CreateProductCommand).Assembly       // Application Assembly
    );
});

// Add FastEndpoints
builder.Services.AddFastEndpoints()
    .AddOpenApi();

// Add Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMessagingInfrastructure(builder.Configuration);

// Add Azure services
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(
        builder.Configuration["StorageConnection:blobServiceUri"])
        .WithName("StorageConnection");
});

builder.Services.AddScoped<ICategorySeeder, CategoryDbSeeder>();
builder.Services.AddScoped<IProductSeeder, ProductDbSeeder>();
builder.Services.AddScoped<IDbSeeder, MainDbSeeder>();

var app = builder.Build();

// Configure middleware pipeline
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

app.UseCors("CorsPolicy");
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
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
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

await app.RunAsync();
