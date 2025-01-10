using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Admin.Application.Common.Exceptions;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Admin.Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICacheService _cacheService;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        IDateTimeProvider dateTimeProvider,
        ICacheService cacheService,
        ILogger<JwtTokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _dateTimeProvider = dateTimeProvider;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TokenResult> GenerateTokenAsync(
        string userId,
        string username,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permissions
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var now = _dateTimeProvider.UtcNow;
            var accessTokenExpiration = now.AddMinutes(_jwtSettings.ExpiryInMinutes);
            var refreshTokenExpiration = now.AddDays(_jwtSettings.RefreshTokenExpiryInDays);

            // Generate access token
            var accessToken = GenerateAccessToken(claims, now, accessTokenExpiration);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();

            // Store refresh token in cache
            await StoreRefreshTokenAsync(userId, refreshToken, refreshTokenExpiration);

            return new TokenResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration,
                Permissions = permissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user {UserId}", userId);
            throw new AppException("Token.GenerationFailed", "Failed to generate authentication token");
        }
    }

    public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Get stored refresh token
            var storedToken = await _cacheService.GetAsync<RefreshToken>($"refresh_token:{refreshToken}");

            if (storedToken == null || !storedToken.IsActive)
            {
                throw new AppException("Token.Invalid", "Invalid refresh token");
            }

            // Get user info (you might want to inject a user service here)
            // For now, we'll just use the stored user ID
            var userId = storedToken.UserId;

            // Revoke the old refresh token
            await RevokeTokenAsync(userId);

            // Generate new tokens (you'll need to get the user's roles and permissions again)
            // This is simplified - you should get the actual user data
            return await GenerateTokenAsync(userId, "username", new[] { "User" }, new[] { "BasicAccess" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw new AppException("Token.RefreshFailed", "Failed to refresh token");
        }
    }

    public async Task RevokeTokenAsync(string userId)
    {
        try
        {
            // Remove refresh token from cache
            await _cacheService.RemoveAsync($"user_refresh_token:{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token for user {UserId}", userId);
            throw new AppException("Token.RevokeFailed", "Failed to revoke token");
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Check if token is in the blacklist
            //var isBlacklisted = await _cacheService.GetAsync<bool>($"blacklisted_token:{token}");
            //return !isBlacklisted;

            var isBlacklisted = await _cacheService.GetAsync<string>($"blacklisted_token:{token}");
            return isBlacklisted != null && bool.Parse(isBlacklisted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims, DateTime notBefore, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: notBefore,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task StoreRefreshTokenAsync(string userId, string refreshToken, DateTime expiration)
    {
        var token = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiryDate = expiration
        };

        // Store in cache
        await _cacheService.SetAsync(
            $"refresh_token:{refreshToken}",
            token,
            expiration - _dateTimeProvider.UtcNow);

        // Store reference to user's refresh token
        await _cacheService.SetAsync(
            $"user_refresh_token:{userId}",
            refreshToken,
            expiration - _dateTimeProvider.UtcNow);
    }
}