using Microsoft.AspNetCore.Authorization;

namespace Admin.WebAPI.Infrastructure.Authorization;

// Attribute for permission-based authorization
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(policy: $"Permission:{permission}")
    {
    }
}
