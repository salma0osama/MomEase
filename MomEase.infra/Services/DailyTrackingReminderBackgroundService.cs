using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MomEase.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MomEase.infra.Services
{
    /// <summary>
    /// Background service that sends daily tracking reminders
    /// Runs once per day at 8:00 AM
    /// </summary>
    public class DailyTrackingReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyTrackingReminderBackgroundService> _logger;
        private DateTime _nextRunTime;

        public DailyTrackingReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DailyTrackingReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _nextRunTime = GetNextRunTime();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 DailyTrackingReminderBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now.AddHours(1);  

                if (now >= _nextRunTime)
                {
                    try
                    {
                        _logger.LogInformation("⏰ Running daily tracking reminders at {Time}", now);

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var trackingReminderService = scope.ServiceProvider
                                .GetRequiredService<IDailyTrackingReminderService>();

                            await trackingReminderService.SendDailyTrackingRemindersAsync();
                        }

                        _nextRunTime = GetNextRunTime();
                        _logger.LogInformation("✅ Daily tracking reminders completed. Next run at {NextTime}", _nextRunTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error in daily tracking reminders");
                    }
                }

                // ⏰ Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("🛑 DailyTrackingReminderBackgroundService stopped");
        }

        /// <summary>
        /// Calculate next run time (8:00 AM)
        /// </summary>
        private DateTime GetNextRunTime()
        {
            var now = DateTime.Now.AddHours(1);
            var next = now.Date.AddHours(8);  // 6:00 PM => 18 // 8:00 AM
            //var next = now.AddSeconds(10);
            if (next <= now)
                next = next.AddDays(1);  // Next day if already passed

                return next;
        }
    }
}
