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

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var userName = Context.User.Identity.Name;
            var groupJoinMessage = $"{userName} присоединился к группе \"{groupName}\"";
            await Clients.Group(groupName).SendAsync("GroupJoinNotification", groupJoinMessage);
        }

        public async Task SendGroupMessage(string groupName, string message)
        {
            var userName = Context.User.Identity.Name;
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            await Clients.Group(groupName).SendAsync("Receive", message, userName, timestamp);
        }

        [Authorize(Roles = "admin")]
        public async Task Notify(string message)
        {
            await Clients.All.SendAsync("Receive", message, "Администратор");
        }
    }
}