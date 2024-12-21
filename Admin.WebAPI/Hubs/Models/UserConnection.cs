namespace Admin.WebAPI.Hubs.Models;

public class UserConnection
{
    public string? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}