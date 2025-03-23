namespace Admin.Application.Common.Interfaces;
public record TokenResult
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiration { get; init; }
    public DateTime RefreshTokenExpiration { get; init; }
    public IEnumerable<string> Permissions { get; init; } = Array.Empty<string>();
}
