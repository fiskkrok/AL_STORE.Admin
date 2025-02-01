using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Admin.Application.Common.Settings;
using Admin.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Admin.WebAPI.Infrastructure;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
      
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "ApiKey"; // Change default scheme
            options.DefaultChallengeScheme = "ApiKey";
        })
        .AddJwtBearer("Bearer", options =>
            {
                options.Authority = configuration["IdentityServer:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = new[] { "api", "admin_api" }, // Add both valid audiences
                    ValidateIssuer = true,
                    ValidIssuer = configuration["IdentityServer:Authority"],
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
            }).AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

        return services;
    }
}

public static class AuthorizationConfiguration
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
       
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
                    )))
            .AddPolicy("StoreAccess", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c =>
                    c is { Type: "scope", Value: "products.read" } ||
                    c is { Type: "scope", Value: "categories.read" }
                )));

        return services;
    }
}
public static class AuthorizationPolicyExtensions
{
    public static AuthorizationOptions AddResourcePolicy(
        this AuthorizationOptions options,
        string resource,
        string permission)
    {
        options.AddPolicy($"{resource}.{permission}", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim(c => c.Type == "Permission" &&
                                           (c.Value == $"{resource}.{permission}" ||
                                            c.Value == $"{resource}.Manage" ||
                                            c.Value == Roles.Admin))));

        return options;
    }
}

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/products/bulk"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            _logger.LogWarning("API key missing in request");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
            return;
        }

        var allowedStores = _configuration.GetSection("StoreIntegration:AllowedStores")
            .Get<List<StoreConfiguration>>();

        var store = allowedStores?.FirstOrDefault(s => s.ApiKey == apiKey.ToString());

        if (store == null)
        {
            _logger.LogWarning("Invalid API key used: {ApiKey}", apiKey);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // Add store info to the context for logging
        context.Items["StoreName"] = store.Name;
        await _next(context);
    }
}

public class StoreConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public List<string> AllowedScopes { get; set; } = new();
}
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.ToString();
        var allowedStores = _configuration.GetSection("StoreIntegration:AllowedStores")
            .Get<List<StoreConfiguration>>();

        var store = allowedStores?.FirstOrDefault(s => s.ApiKey == providedApiKey);

        if (store == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, store.Name),
            new Claim("scope", "products.read"),
            new Claim("scope", "categories.read")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}