using Microsoft.AspNetCore.Authorization; // для атрибута Authorize
using Microsoft.AspNetCore.SignalR;

namespace SignalRApp
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Receive", message, userName);
        }
        [Authorize(Roles = "admin")]
        public async Task Notify(string message)
        {
            await Clients.All.SendAsync("Receive", message, "Администратор");
        }
    }
}