using System.Security.Claims;
using System.Text.Encodings.Web;

using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Settings;
using Admin.Infrastructure.Services;
using Admin.WebAPI.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Authentication services configuration
/// </summary>
public static class AuthenticationServicesConfiguration
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Add token service
        services.AddScoped<ITokenService, TokenService>();

        // Configure authentication with multiple schemes - but don't set defaults
        // This lets each endpoint specify which scheme(s) it accepts
        services.AddAuthentication()
            .AddJwtBearer(AuthConstants.JwtBearerScheme, options =>
            {
                options.Authority = configuration["IdentityServer:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = new[] { "api", "admin_api" },
                    ValidateIssuer = true,
                    ValidIssuer = configuration["IdentityServer:Authority"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
                options.Events = ConfigureJwtBearerEvents();
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                AuthConstants.ApiKeyScheme, _ => { });

        return services;
    }

    private static JwtBearerEvents ConfigureJwtBearerEvents()
    {
        return new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "JWT authentication failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JWT token validated for {Subject}",
                    context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier));
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                // Special handling for SignalR
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    (context.HttpContext.Request.Path.StartsWithSegments("/hubs")))
                {
                    // Read token from query string for SignalR connections
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    }
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthenticationHandler> _logger;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _configuration = configuration;
        _logger = logger.CreateLogger<ApiKeyAuthenticationHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key is in the request header
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
            _logger.LogWarning("Invalid API key: {ApiKey}", providedApiKey);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        _logger.LogInformation("API key authenticated: {StoreName}", store.Name);

        // Create claims similar to JWT for consistent authorization
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, store.Name),
            new(ClaimTypes.NameIdentifier, store.Name),
            // Add standard scopes for API keys
            new("scope", AuthConstants.ProductsRead),
            new("scope", AuthConstants.CategoriesRead)
        };

        // Add additional configured scopes
        foreach (var scope in store.AllowedScopes)
        {
            if (!claims.Exists(c => c.Type == "scope" && c.Value == scope))
            {
                claims.Add(new Claim("scope", scope));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class StoreConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public List<string> AllowedScopes { get; set; } = new();
}