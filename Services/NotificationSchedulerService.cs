using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TicketAPI.Models;

namespace TicketAPI.Services
{
    public class NotificationSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationSchedulerService> _logger;

        public NotificationSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<NotificationSchedulerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Scheduler Service запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRunTime = now.Date.AddHours(12).AddMinutes(0);

                    if (now > nextRunTime)
                    {
                        nextRunTime = nextRunTime.AddDays(1);
                    }

                    var delay = nextRunTime - now;

                    _logger.LogInformation($"Следующий запуск в {nextRunTime} (через {delay.TotalHours} часов)");

                    await Task.Delay(delay, stoppingToken);

                    _logger.LogInformation("Запуск генерации уведомлений...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
                        var generator = new NotificationGenerator(dbContext);
                        await generator.GenerateReminderAndFeedbackNotifications();
                    }

                    _logger.LogInformation("Генерация уведомлений завершена.");
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в Notification Scheduler Service");

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
