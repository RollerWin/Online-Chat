using Microsoft.AspNetCore.Authorization; // для атрибута Authorize
using Microsoft.AspNetCore.SignalR;

namespace SignalRApp
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task Send(string message)
        {
            var userName = Context.User.Identity.Name; // Получаем имя пользователя из авторизованного контекста
            var timestamp = DateTime.Now.ToString("HH:mm:ss"); // Получаем текущее время
            await Clients.All.SendAsync("Receive", message, userName, timestamp);
        }
        [Authorize(Roles = "admin")]
        public async Task Notify(string message)
        {
            await Clients.All.SendAsync("Receive", message, "Администратор");
        }
    }
}