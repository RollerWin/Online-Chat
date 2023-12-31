﻿using Microsoft.AspNetCore.Authorization; // для атрибута Authorize
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRMetanit;
using System.Security.Claims;

namespace SignalRApp
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly OnlineChatContext _dbContext;

        public ChatHub(OnlineChatContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ConnectedUsers");
            await Clients.Caller.SendAsync("UpdateUserRole", Context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value);
        }

        public async Task Send(string message)
        {
            var userName = Context.User.Identity.Name;

            if (IsUserBanned(userName))
            {
                // Отправить уведомление о невозможности отправки сообщения
                await Clients.Caller.SendAsync("Receive", "Вы забанены и не можете отправлять сообщения.", "Администратор");
                return;
            }
            else
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                await Clients.All.SendAsync("Receive", message, userName, timestamp);
            }
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

            if (IsUserBanned(userName))
            {
                // Отправить уведомление о невозможности отправки сообщения
                await Clients.Caller.SendAsync("Receive", "Вы забанены и не можете отправлять сообщения в группе.", "Администратор");
                return;
            }
            else
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                await Clients.Group(groupName).SendAsync("Receive", message, userName, timestamp);
            }
        }

        [Authorize(Roles = "admin")]
        public async Task Notify(string message)
        {
            await Clients.All.SendAsync("Receive", message, "Администратор");
        }

        [Authorize(Roles = "admin")]
        public async Task BanUser(string userName)
        {

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == userName);

            if (user != null)
            {
                user.IsBanned = true;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                var bannedUserId = Context.ConnectionId;

                await Clients.Client(bannedUserId).SendAsync("ForceDisconnect");

                // Отправляем сообщение всем пользователям о бане
                await Clients.All.SendAsync("Receive", $"{userName} был забанен.", "Администратор");

            }
            else
            {
                await Clients.Caller.SendAsync("Receive", $"Пользователь {userName} не найден.", "Администратор");
            }
        }

        private bool IsUserBanned(string userName)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Name == userName);
            return user?.IsBanned ?? false;
        }


    }
}