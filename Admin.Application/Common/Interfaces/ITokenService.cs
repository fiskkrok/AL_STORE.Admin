namespace Admin.Application.Common.Interfaces;
public interface ITokenService
{
    Task<TokenResult> GenerateTokenAsync(string userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);
    Task<TokenResult> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string userId);
    Task<bool> ValidateTokenAsync(string token);
}
