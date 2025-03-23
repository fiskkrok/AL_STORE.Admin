using Admin.Domain.Common;

namespace Admin.Domain.Entities;

public class User : AuditableEntity
{
    private readonly List<string> _roles = new();
    private readonly List<string> _permissions = new();

    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTime? LastLoginAt { get; private set; }
    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();

    private User() { } // For EF Core

    public User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        SetModified(null);
    }

    public void UpdateEmail(string newEmail)
    {
        Email = newEmail;
        SetModified(null);
    }

    public void AddRole(string role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
            SetModified(null);
        }
    }

    public void RemoveRole(string role)
    {
        if (_roles.Remove(role))
        {
            SetModified(null);
        }
    }

    public void AddPermission(string permission)
    {
        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
            SetModified(null);
        }
    }

    public void RemovePermission(string permission)
    {
        if (_permissions.Remove(permission))
        {
            SetModified(null);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        Status = UserStatus.Inactive;
        SetModified(null);
    }

    public void Activate()
    {
        IsActive = true;
        Status = UserStatus.Active;
        SetModified(null);
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetModified(null);
    }
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    PendingVerification
}

