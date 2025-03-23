using System.Security.Cryptography;

using Admin.Application.Common.Constants;
using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Admin.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;

namespace Admin.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            throw new AppException(ErrorCodes.DatabaseError, "Error retrieving user");
        }
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetByUsernameAsync(username, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username {Username}", username);
            throw new AppException(ErrorCodes.DatabaseError, "Error retrieving user");
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email {Email}", email);
            throw new AppException(ErrorCodes.DatabaseError, "Error retrieving user");
        }
    }

    public async Task<(User User, TokenResult Tokens)> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByUsernameAsync(username, cancellationToken);
            if (user == null)
            {
                throw new AppException(ErrorCodes.InvalidCredentials, "Invalid username or password");
            }

            if (!user.IsActive)
            {
                throw new AppException(ErrorCodes.Unauthorized, "User account is inactive");
            }

            if (!await ValidatePasswordAsync(user, password))
            {
                throw new AppException(ErrorCodes.InvalidCredentials, "Invalid username or password");
            }

            // Update last login
            user.UpdateLastLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate tokens
            var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
            var tokens = await _tokenService.GenerateTokenAsync(
                user.Id.ToString(),
                user.Username,
                user.Roles,
                permissions);

            return (user, tokens);
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", username);
            throw new AppException(ErrorCodes.InternalServerError, "Authentication failed");
        }
    }

    public async Task<User> CreateUserAsync(
        string username,
        string email,
        string password,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if username or email already exists
            if (await GetByUsernameAsync(username, cancellationToken) != null)
            {
                throw new AppException(ErrorCodes.ResourceAlreadyExists, "Username already exists");
            }

            if (await GetByEmailAsync(email, cancellationToken) != null)
            {
                throw new AppException(ErrorCodes.ResourceAlreadyExists, "Email already exists");
            }

            var passwordHash = HashPassword(password);
            var user = new User(username, email, passwordHash);

            // Add roles
            foreach (var role in roles)
            {
                user.AddRole(role);
            }

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return user;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", username);
            throw new AppException(ErrorCodes.InternalServerError, "Failed to create user");
        }
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", user.Id);
            throw new AppException(ErrorCodes.InternalServerError, "Failed to update user");
        }
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new AppException(ErrorCodes.ResourceNotFound, "User not found");
            }

            user.Deactivate();
            await UpdateUserAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            throw new AppException(ErrorCodes.InternalServerError, "Failed to delete user");
        }
    }

    public Task<bool> ValidatePasswordAsync(User user, string password)
    {
        var hashedPassword = HashPassword(password, GetExistingSalt(user.PasswordHash));
        return Task.FromResult(hashedPassword == user.PasswordHash);
    }

    public async Task UpdatePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new AppException(ErrorCodes.ResourceNotFound, "User not found");
            }

            if (!await ValidatePasswordAsync(user, currentPassword))
            {
                throw new AppException(ErrorCodes.InvalidCredentials, "Current password is incorrect");
            }

            user.UpdatePassword(HashPassword(newPassword));
            await UpdateUserAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user {UserId}", userId);
            throw new AppException(ErrorCodes.InternalServerError, "Failed to update password");
        }
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new AppException(ErrorCodes.ResourceNotFound, "User not found");
            }

            // Combine role-based permissions and direct permissions
            var permissions = new HashSet<string>(user.Permissions);

            // Add role-based permissions
            foreach (var role in user.Roles)
            {
                // You might want to inject a role service here to get role permissions
                // For now, we'll just add some basic permissions based on roles
                switch (role.ToLower())
                {
                    case "admin":
                        permissions.Add("Admin");
                        break;
                    case "manager":
                        permissions.Add("Products.Manage");
                        permissions.Add("Orders.Manage");
                        break;
                    case "user":
                        permissions.Add("Products.View");
                        permissions.Add("Orders.View");
                        break;
                }
            }

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            throw new AppException(ErrorCodes.InternalServerError, "Failed to retrieve user permissions");
        }
    }

    private static string HashPassword(string password, byte[]? salt = null)
    {
        salt ??= new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    private static byte[] GetExistingSalt(string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        return Convert.FromBase64String(parts[0]);
    }
}