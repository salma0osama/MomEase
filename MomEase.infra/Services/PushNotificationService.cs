using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google;
using Microsoft.Extensions.Logging;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MomEase.infra.Data;
using Microsoft.EntityFrameworkCore;

namespace MomEase.infra.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly MomEaseDbContext _context;
        private readonly ILogger<PushNotificationService> _logger;
        private static bool _firebaseInitialized = false;
        private static readonly object _lock = new object();

        public PushNotificationService(
            MomEaseDbContext context,
            ILogger<PushNotificationService> logger)
        {
            _context = context;
            _logger = logger;

            // Initialize Firebase مرة واحدة بس
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (_firebaseInitialized) return;

            lock (_lock)
            {
                if (_firebaseInitialized) return;

                try
                {
                    var credentialPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "firebase-adminsdk.json");

                    if (!File.Exists(credentialPath))
                    {
                        _logger.LogWarning(
                            "⚠️ Firebase credentials file not found at: {Path}. Push notifications will be disabled.",
                            credentialPath);
                        return;
                    }

                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = GoogleCredential.FromFile(credentialPath)
                        });

                        _logger.LogInformation("✅ Firebase Admin SDK initialized successfully");
                    }

                    _firebaseInitialized = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to initialize Firebase Admin SDK");
                }
            }
        }

        public async Task SendPushNotificationAsync(
            int userId,
            string title,
            string body,
            string type,
            int? relatedEntityId = null)
        {
            if (!_firebaseInitialized)
            {
                _logger.LogWarning(
                    "⚠️ Firebase not initialized. Skipping push notification for user {UserId}",
                    userId);
                return;
            }

            try
            {
                // جيب كل الـ Device Tokens للـ User ده
                var deviceTokens = await _context.DeviceTokens
                    .AsNoTracking()
                    .Where(dt => dt.UserId == userId && dt.IsActive)
                    .Select(dt => dt.Token)
                    .ToListAsync();

                if (!deviceTokens.Any())
                {
                    _logger.LogInformation(
                        "ℹ️ No active device tokens found for user {UserId}. Skipping push notification.",
                        userId);
                    return;
                }

                _logger.LogInformation(
                    "📲 Sending push notification to {Count} device(s) for user {UserId}",
                    deviceTokens.Count, userId);

                // بعت Notification لكل Device
                var tasks = deviceTokens.Select(token =>
                    SendToDeviceAsync(token, title, body, type, relatedEntityId));

                await Task.WhenAll(tasks);

                _logger.LogInformation(
                    "✅ Push notifications sent to user {UserId}",
                    userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to send push notification to user {UserId}",
                    userId);
                // لا نرمي Exception - الـ Notification اتحفظت في Database
            }
        }

        private async Task SendToDeviceAsync(
            string deviceToken,
            string title,
            string body,
            string type,
            int? relatedEntityId)
        {
            try
            {
                var message = new Message()
                {
                    Token = deviceToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "type", type },
                        { "relatedEntityId", relatedEntityId?.ToString() ?? "" },
                        { "click_action", "FLUTTER_NOTIFICATION_CLICK" }
                    },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Sound = "default",
                            ChannelId = "momease_notifications",
                            Icon = "ic_notification",
                            Color = "#FF6B9D" // لون الأيقونة
                        }
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = "default",
                            Badge = 1,
                            Alert = new ApsAlert()
                            {
                                Title = title,
                                Body = body
                            }
                        }
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                _logger.LogInformation(
                    "📲 Push notification sent successfully: {Response}",
                    response);
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                // الـ Token منتهي أو الأبلكيشن اتمسح
                _logger.LogWarning(
                    "⚠️ Device token is invalid or unregistered: {Token}. Marking as inactive.",
                    deviceToken);

                // علّم الـ Token كـ inactive
                var tokenEntity = await _context.DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.Token == deviceToken);

                if (tokenEntity != null)
                {
                    tokenEntity.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Failed to send push notification to device: {Token}",
                    deviceToken);
            }
        }
    }
}
