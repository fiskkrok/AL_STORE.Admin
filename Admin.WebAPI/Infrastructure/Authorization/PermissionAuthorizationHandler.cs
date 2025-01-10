using Admin.Application.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Admin.WebAPI.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRoleService _roleService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IRoleService roleService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userPermissions = context.User.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value);

        if (_roleService.HasRequiredPermissions(userPermissions, requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "User {UserId} denied access to {Permission}",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                requirement.Permission);
        }

        return Task.CompletedTask;
    }
}
