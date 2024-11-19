using Microsoft.AspNetCore.SignalR;

namespace Admin.WebAPI.Hubs;

public class NotificationHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}