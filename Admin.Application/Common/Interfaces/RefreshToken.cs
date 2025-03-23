namespace Admin.Application.Common.Interfaces;
public record RefreshToken
{
    public string Token { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTime ExpiryDate { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsExpired;
}
