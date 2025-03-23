using Admin.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Admin.Application.Services;
public class RoleService : IRoleService
{
    private readonly ILogger<RoleService> _logger;

    public RoleService(ILogger<RoleService> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> GetAvailableRoles()
    {
        return Roles.RolePermissions.Keys;
    }

    public IEnumerable<string> GetRolePermissions(string role)
    {
        if (!Roles.RolePermissions.TryGetValue(role, out var permissions))
        {
            _logger.LogWarning("Attempted to get permissions for invalid role: {Role}", role);
            return Array.Empty<string>();
        }

        return permissions;
    }

    public bool IsValidRole(string role)
    {
        return Roles.RolePermissions.ContainsKey(role);
    }

    public bool HasRequiredPermissions(IEnumerable<string> userPermissions, string requiredPermission)
    {
        var userPermissionSet = new HashSet<string>(userPermissions);

        // Check if user has exact permission
        if (userPermissionSet.Contains(requiredPermission))
            return true;

        // Check if user has manage permission for the resource
        var resourceType = requiredPermission.Split('.')[0];
        var managePermission = $"{resourceType}.Manage";

        return userPermissionSet.Contains(managePermission) || userPermissionSet.Contains(Roles.Admin);
    }
}
