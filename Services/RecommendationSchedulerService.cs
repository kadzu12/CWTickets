using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class RecommendationSchedulerService : BackgroundService
{
    private readonly ILogger<RecommendationSchedulerService> _logger;

    public RecommendationSchedulerService(ILogger<RecommendationSchedulerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recommendation Scheduler Service запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Запуск генерации рекомендаций...");

                var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "Recommendation", "RecommendationProject");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(projectPath, "venv", "Scripts", "python.exe"),
                        Arguments = "main.py",
                        WorkingDirectory = projectPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Рекомендации успешно обновлены.");
                    _logger.LogInformation(output);
                }
                else
                {
                    _logger.LogError($"Ошибка при выполнении скрипта: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RecommendationSchedulerService.");
            }

            // Ждём 5 часов до следующего запуска
            await Task.Delay(TimeSpan.FromHours(5), stoppingToken);
        }
    }
}
