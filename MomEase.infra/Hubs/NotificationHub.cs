using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MomEase.infra.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // إضافة المستخدم لـ Group خاص بيه
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"✅ User {userId} connected to notifications (ConnectionId: {Context.ConnectionId})");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"❌ User {userId} disconnected from notifications");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method للـ Client يقدر يطلب mark as read
        public async Task MarkAsRead(int notificationId)
        {
            await Clients.Caller.SendAsync("NotificationMarkedAsRead", notificationId);
        }
    }
}