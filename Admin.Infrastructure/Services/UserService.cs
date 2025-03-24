using System.Security.Cryptography;

using Admin.Application.Common.Constants;
using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Interfaces;
using Admin.Domain.Entities;
using Admin.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Admin.Infrastructure.Services;

public class UserService : ServiceBase, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(
            () => _userRepository.GetByIdAsync(id, cancellationToken),
            ErrorCodes.DatabaseError,
            $"Error retrieving user with ID {id}");
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(
            () => _userRepository.GetByUsernameAsync(username, cancellationToken),
            ErrorCodes.DatabaseError,
            $"Error retrieving user with username {username}");
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithErrorHandlingAsync(
            () => _userRepository.GetByEmailAsync(email, cancellationToken),
            ErrorCodes.DatabaseError,
            $"Error retrieving user with email {email}");
    }

    public async Task<(User User, TokenResult Tokens)> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Get user
        var user = await GetByUsernameAsync(username, cancellationToken);
        if (user == null)
        {
            throw new AppException(ErrorCodes.InvalidCredentials, "Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new AppException(ErrorCodes.Unauthorized, "User account is inactive");
        }

        // Validate password
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

    public async Task<User> CreateUserAsync(string username, string email, string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.UsernameExistsAsync(username, cancellationToken))
        {
            throw new AppException(ErrorCodes.UsernameAlreadyExists, "Username already exists");
        }

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new AppException(ErrorCodes.EmailAlreadyExists, "Email already exists");
        }

        var passwordHash = HashPassword(password);
        var user = new User(username, email, passwordHash);

        foreach (var role in roles)
        {
            user.AddRole(role);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new AppException(ErrorCodes.UserNotFound, "User not found");
        }

        await _userRepository.RemoveAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ValidatePasswordAsync(User user, string password)
    {
        var salt = GetExistingSalt(user.PasswordHash);
        var hashedPassword = HashPassword(password, salt);
        return user.PasswordHash == hashedPassword;
    }

    public async Task UpdatePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new AppException(ErrorCodes.UserNotFound, "User not found");
        }

        if (!await ValidatePasswordAsync(user, currentPassword))
        {
            throw new AppException(ErrorCodes.InvalidCredentials, "Current password is incorrect");
        }

        var newPasswordHash = HashPassword(newPassword);
        user.UpdatePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new AppException(ErrorCodes.UserNotFound, "User not found");
        }

        return user.Permissions;
    }

 

    private string HashPassword(string password, byte[]? salt = null)
    {
        // Password hashing logic - moved from UserService implementation
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