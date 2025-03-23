using Admin.Domain.Constants;
using Admin.WebAPI.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;

namespace Admin.WebAPI.Configurations;

/// <summary>
/// Authorization services configuration
/// </summary>
public static class AuthorizationServicesConfiguration
{
    /// <summary>
    /// Adds authorization policies and handlers
    /// </summary>
    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        // Add authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Add authorization with policies
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

        // Register permission-based policies
        RegisterPermissionPolicies(services);

        return services;
    }

    /// <summary>
    /// Registers policies for permissions
    /// </summary>
    private static void RegisterPermissionPolicies(IServiceCollection services)
    {
        // Register permission policies
        services.AddAuthorization(options =>
        {
            // Products
            RegisterResourcePolicies(options, "Products", new[] { "View", "Create", "Edit", "Delete" });

            // Orders
            RegisterResourcePolicies(options, "Orders", new[] { "View", "Process", "Refund", "Cancel" });

            // Categories
            RegisterResourcePolicies(options, "Categories", new[] { "View", "Create", "Edit", "Delete" });

            // Users
            RegisterResourcePolicies(options, "Users", new[] { "View", "Create", "Edit", "Delete" });
        });
    }

    /// <summary>
    /// Registers policies for a resource
    /// </summary>
    private static void RegisterResourcePolicies(AuthorizationOptions options, string resource, string[] permissions)
    {
        foreach (var permission in permissions)
        {
            options.AddPolicy($"Permission:{resource}.{permission}", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "Permission" &&
                                              (c.Value == $"{resource}.{permission}" ||
                                               c.Value == $"{resource}.Manage" ||
                                               c.Value == Roles.Admin))));
        }
    }
}