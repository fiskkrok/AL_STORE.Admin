using System.Text;
using Admin.Application.Common.Settings;
using Admin.Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Admin.WebAPI.Infrastructure;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };

            // Configure JWT events
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },

                OnMessageReceived = context =>
                {
                    // Allow JWT token to be passed in query string for SignalR
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}

public static class AuthorizationConfiguration
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // User management
            options.AddPolicy("Users.View", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Users.View ||
                     c.Value == Permissions.Users.Manage ||
                     c.Value == Roles.Admin))));

            options.AddPolicy("Users.Manage", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Users.Manage ||
                     c.Value == Roles.Admin))));

            // Product management
            options.AddPolicy("Products.View", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Products.View ||
                     c.Value == Permissions.Products.Manage ||
                     c.Value == Roles.Admin))));

            options.AddPolicy("Products.Manage", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Products.Manage ||
                     c.Value == Roles.Admin))));

            // Order management
            options.AddPolicy("Orders.View", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Orders.View ||
                     c.Value == Permissions.Orders.Manage ||
                     c.Value == Roles.Admin))));

            options.AddPolicy("Orders.Manage", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Orders.Manage ||
                     c.Value == Roles.Admin))));

            // Category management
            options.AddPolicy("Categories.View", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Categories.View ||
                     c.Value == Permissions.Categories.Manage ||
                     c.Value == Roles.Admin))));

            options.AddPolicy("Categories.Manage", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    (c.Value == Permissions.Categories.Manage ||
                     c.Value == Roles.Admin))));

            // Admin access
            options.AddPolicy("AdminAccess", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                    c.Value == Roles.Admin)));

            // Application-wide policies
            options.AddPolicy("RequireActiveUser", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "IsActive" &&
                    c.Value == "true")));

            // Add default policy

            // Replace .RequirePolicy("RequireActiveUser") with .Combine(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireAssertion(context => context.User.HasClaim(c => c.Type == "IsActive" && c.Value == "true")).Build())

            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Combine(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireAssertion(context => context.User.HasClaim(c => c.Type == "IsActive" && c.Value == "true")).Build())
                .Build();
        });

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
