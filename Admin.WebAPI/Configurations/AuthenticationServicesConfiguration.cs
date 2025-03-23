using System.Security.Claims;
using System.Text.Encodings.Web;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Settings;
using Admin.Infrastructure.Services;
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
    /// <summary>
    /// Adds authentication services, including JWT and API Key authentication
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Add token service
        services.AddScoped<ITokenService, JwtTokenService>();

        // Configure authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "ApiKey";
            options.DefaultChallengeScheme = "ApiKey";
        })
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = configuration["IdentityServer:Authority"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudiences = new[] { "api", "admin_api" },
                ValidateIssuer = true,
                ValidIssuer = configuration["IdentityServer:Authority"],
                ValidateLifetime = true
            };
            options.Events = ConfigureJwtBearerEvents();
        })
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });

        return services;
    }

    /// <summary>
    /// Configures JWT Bearer events for logging and debugging
    /// </summary>
    private static JwtBearerEvents ConfigureJwtBearerEvents()
    {
        return new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("OnChallenge: {Error}", context.Error);
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token received: {Token}", context.Token);
                return Task.CompletedTask;
            }
        };
    }
}

/// <summary>
/// API Key authentication handler
/// </summary>
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

/// <summary>
/// Store configuration model used for API Key authentication
/// </summary>
public class StoreConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public List<string> AllowedScopes { get; set; } = new();
}