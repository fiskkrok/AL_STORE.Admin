using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Admin.Application.Common.Interfaces;
using Admin.Application.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Admin.Infrastructure.Services;

public class TokenService : ServiceBase, ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICacheService _cacheService;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IDateTimeProvider dateTimeProvider,
        ICacheService cacheService)
    {
        _jwtSettings = jwtSettings.Value;
        _dateTimeProvider = dateTimeProvider;
        _cacheService = cacheService;
    }

    public async Task<TokenResult> GenerateTokenAsync(
        string userId,
        string username,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
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
        }, "Token.GenerationFailed", "Failed to generate authentication token");
    }

    public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public async Task RevokeTokenAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        throw new NotImplementedException();
    }

    // Remaining methods with similar pattern...
    // RefreshTokenAsync, RevokeTokenAsync, ValidateTokenAsync, etc.

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