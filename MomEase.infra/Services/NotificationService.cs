using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.NotificationDto;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IPushNotificationService _pushService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repo,
            IHubContext<NotificationHub> hubContext,
            IPushNotificationService pushService,
            ILogger<NotificationService> logger)
        {
            _repo = repo;
            _hubContext = hubContext;
            _pushService = pushService;
            _logger = logger;
        }

        public async Task SendRealtimeNotificationAsync(
            int userId,
            string title,
            string body,
            string type,
            int? relatedEntityId = null,
            string? actionUrl = null)
        {
            try
            {
                _logger.LogInformation(
                    "📤 Sending notification to user {UserId}: {Title}",
                    userId, title);

                // ✅ Step 1: حفظ في الـ Database
                var notification = new Notifications
                {
                    UserId = userId,
                    Title = title,
                    Body = body,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    ActionUrl = actionUrl,
                    IsRead = false,
                    CreatedAt = DateTime.Now.AddHours(1)
                };

                var saved = await _repo.CreateAsync(notification);

                _logger.LogInformation(
                    "💾 Notification saved to database with ID {NotificationId}",
                    saved.NotificationId);

                // ✅ Step 2: إرسال عبر SignalR (للمستخدمين Online)
                try
                {
                    await _hubContext.Clients
                        .Group($"User_{userId}")
                        .SendAsync("ReceiveNotification", new
                        {
                            notificationId = saved.NotificationId,
                            title = saved.Title,
                            body = saved.Body,
                            type = saved.Type,
                            relatedEntityId = saved.RelatedEntityId,
                            actionUrl = saved.ActionUrl,
                            createdAt = saved.CreatedAt
                        });

                    _logger.LogInformation(
                        "✅ SignalR notification sent to user {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "⚠️ SignalR delivery failed for user {UserId} (user might be offline)",
                        userId);
                    // لا نرمي Exception - الـ User ممكن يكون Offline
                }

                // ✅ Step 3: إرسال Push Notification (للموبايل)
                try
                {
                    await _pushService.SendPushNotificationAsync(
                        userId,
                        title,
                        body,
                        type,
                        relatedEntityId);

                    _logger.LogInformation(
                        "📲 Push notification sent to user {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "❌ Push notification failed for user {UserId}", userId);
                    // لا نرمي Exception - SignalR ممكن يكون اشتغل
                }

                Console.WriteLine($"🔔 Notification sent to User {userId}: {title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to send notification to user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            try
            {
                var notifications = await _repo.GetByUserIdAsync(userId);
                _logger.LogInformation(
                    "📥 Retrieved {Count} notifications for user {UserId}",
                    notifications.Count(), userId);
                return notifications.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to get notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                var count = await _repo.GetUnreadCountAsync(userId);
                _logger.LogInformation(
                    "📊 User {UserId} has {Count} unread notifications",
                    userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to get unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
        {
            try
            {
                var notification = await _repo.GetByIdAsync(userId, notificationId);
                if (notification == null)
                {
                    _logger.LogWarning(
                        "⚠️ Notification {NotificationId} not found for user {UserId}",
                        notificationId, userId);
                    return false;
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.Now.AddHours(1);

                await _repo.UpdateAsync(notification);

                _logger.LogInformation(
                    "✅ Notification {NotificationId} marked as read for user {UserId}",
                    notificationId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to mark notification {NotificationId} as read for user {UserId}",
                    notificationId, userId);
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var result = await _repo.MarkAllAsReadAsync(userId);

                _logger.LogInformation(
                    "✅ All notifications marked as read for user {UserId}",
                    userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to mark all notifications as read for user {UserId}",
                    userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int userId, int notificationId)
        {
            try
            {
                var result = await _repo.DeleteAsync(userId, notificationId);

                if (result)
                {
                    _logger.LogInformation(
                        "🗑️ Notification {NotificationId} deleted for user {UserId}",
                        notificationId, userId);
                }
                else
                {
                    _logger.LogWarning(
                        "⚠️ Notification {NotificationId} not found for user {UserId}",
                        notificationId, userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to delete notification {NotificationId} for user {UserId}",
                    notificationId, userId);
                throw;
            }
        }

        // ── MAPPER ────────────────────────────────────────
        private static NotificationDto MapToDto(Notifications n) => new()
        {
            NotificationId = n.NotificationId,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type,
            RelatedEntityId = n.RelatedEntityId,
            ActionUrl = n.ActionUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        };
    }

}
