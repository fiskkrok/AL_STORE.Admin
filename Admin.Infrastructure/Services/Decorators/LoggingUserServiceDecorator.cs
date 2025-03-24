using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;

using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Services.Decorators;
public class LoggingUserServiceDecorator : IUserService
{
    private readonly IUserService _inner;
    private readonly ILogger<LoggingUserServiceDecorator> _logger;

    public LoggingUserServiceDecorator(IUserService inner, ILogger<LoggingUserServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by ID {UserId}", id);
        var result = await _inner.GetByIdAsync(id, cancellationToken);
        _logger.LogDebug("Retrieved user by ID {UserId}: {ResultFound}", id, result != null);
        return result;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by username {Username}", username);
        var result = await _inner.GetByUsernameAsync(username, cancellationToken);
        _logger.LogDebug("Retrieved user by username {Username}: {ResultFound}", username, result != null);
        return result;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting user by email {Email}", email);
        var result = await _inner.GetByEmailAsync(email, cancellationToken);
        _logger.LogDebug("Retrieved user by email {Email}: {ResultFound}", email, result != null);
        return result;
    }

    public async Task<(User User, TokenResult Tokens)> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Authenticating user {Username}", username);
        var result = await _inner.AuthenticateAsync(username, password, cancellationToken);
        _logger.LogDebug("Authenticated user {Username}: {ResultFound}", username, result.User != null);
        return result;
    }

    public async Task<User> CreateUserAsync(string username, string email, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating user {Username}", username);
        var result = await _inner.CreateUserAsync(username, email, password, roles, cancellationToken);
        _logger.LogDebug("Created user {Username}", username);
        return result;
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating user {UserId}", user.Id);
        await _inner.UpdateUserAsync(user, cancellationToken);
        _logger.LogDebug("Updated user {UserId}", user.Id);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting user {UserId}", userId);
        await _inner.DeleteUserAsync(userId, cancellationToken);
        _logger.LogDebug("Deleted user {UserId}", userId);
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        _logger.LogDebug("Validating password for user {UserId}", user.Id);
        var result = await _inner.ValidatePasswordAsync(user, password);
        _logger.LogDebug("Validated password for user {UserId}: {Result}", user.Id, result);
        return result;
    }

    public async Task UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating password for user {UserId}", userId);
        await _inner.UpdatePasswordAsync(userId, currentPassword, newPassword, cancellationToken);
        _logger.LogDebug("Updated password for user {UserId}", userId);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting permissions for user {UserId}", userId);
        var result = await _inner.GetUserPermissionsAsync(userId, cancellationToken);
        _logger.LogDebug("Retrieved permissions for user {UserId}", userId);
        return result;
    }


}
