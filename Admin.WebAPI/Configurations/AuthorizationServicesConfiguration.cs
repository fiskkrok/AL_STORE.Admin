using Admin.WebAPI.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;

namespace Admin.WebAPI.Configurations;

public static class AuthorizationServicesConfiguration
{
    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        // Add authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Add unified policies that work with both JWT and API Key auth
        services.AddAuthorizationBuilder()
            // Admin access policy
            .AddPolicy(AuthConstants.IsAdminPolicy, policy =>
                policy.RequireRole(AuthConstants.SystemAdministratorRole))

            // Product policies
            .AddPolicy(AuthConstants.CanManageProductsPolicy, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c is { Type: "scope", Value: AuthConstants.ProductsCreate or 
                            AuthConstants.ProductsUpdate or 
                            AuthConstants.ProductsDelete or 
                            AuthConstants.FullApiAccess
                        }) ||
                    context.User.IsInRole(AuthConstants.SystemAdministratorRole) ||
                    context.User.IsInRole(AuthConstants.ProductManagerRole)))

            .AddPolicy(AuthConstants.CanReadProductsPolicy, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == "scope" && c.Value is AuthConstants.ProductsRead or AuthConstants.FullApiAccess) ||
                    context.User.IsInRole(AuthConstants.SystemAdministratorRole) ||
                    context.User.IsInRole(AuthConstants.ProductManagerRole)))

            // Category policies
            .AddPolicy(AuthConstants.CanManageCategoriesPolicy, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == "scope" && (
                            c.Value == AuthConstants.CategoriesManage ||
                            c.Value == AuthConstants.FullApiAccess)) ||
                    context.User.IsInRole(AuthConstants.SystemAdministratorRole)))

            .AddPolicy(AuthConstants.CanReadCategoriesPolicy, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == "scope" && (
                            c.Value == AuthConstants.CategoriesRead ||
                            c.Value == AuthConstants.FullApiAccess)) ||
                    context.User.IsInRole(AuthConstants.SystemAdministratorRole)));

        // Map existing policy names to new ones for backward compatibility
        services.AddAuthorizationBuilder()
            .AddPolicy("FullAdminAccess", policy =>
                policy.RequireRole(AuthConstants.SystemAdministratorRole))
            .AddPolicy("ProductsCreate", policy =>
                policy.AddAuthenticationSchemes(AuthConstants.JwtBearerScheme, AuthConstants.ApiKeyScheme)
                .RequireAssertion(context => HasScopeOrRole(context,
                    AuthConstants.ProductsCreate,
                    AuthConstants.FullApiAccess,
                    AuthConstants.SystemAdministratorRole)))
            .AddPolicy("ProductEdit", policy =>
                policy.AddAuthenticationSchemes(AuthConstants.JwtBearerScheme, AuthConstants.ApiKeyScheme)
                .RequireAssertion(context => HasScopeOrRole(context,
                    AuthConstants.ProductsUpdate,
                    AuthConstants.FullApiAccess,
                    AuthConstants.SystemAdministratorRole)));

        return services;
    }

    private static bool HasScopeOrRole(AuthorizationHandlerContext context,
        string specificScope, string fullScope, string adminRole)
    {
        return context.User.HasClaim(c => c.Type == "scope" &&
                   (c.Value == specificScope || c.Value == fullScope)) ||
               context.User.IsInRole(adminRole);
    }
}