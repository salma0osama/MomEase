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
    public class MentalHealthFollowUpBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MentalHealthFollowUpBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // كل ساعة

        public MentalHealthFollowUpBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<MentalHealthFollowUpBackgroundService> _logger)
        {
            _serviceProvider = serviceProvider;
            this._logger = _logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Mental Health Follow-up Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("⏰ Running mental health follow-up checks...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var followUpService = scope.ServiceProvider
                            .GetRequiredService<IMentalHealthFollowUpService>();

                        // 1. بعت Assessment Reminders
                        await followUpService.SendDueAssessmentRemindersAsync();

                        // 2. بعت Tips
                        await followUpService.SendDueTipsAsync();
                    }

                    _logger.LogInformation("✅ Mental health follow-up checks completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in Mental Health Follow-up Background Service");
                }

                // استنى ساعة قبل الـ Check الجاي
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("🛑 Mental Health Follow-up Background Service stopped");
        }
    }
}
