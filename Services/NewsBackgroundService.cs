using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockApiApp.Services
{
    public class NewsBackgroundService : BackgroundService
    {
        private readonly NewsService _newsService;
        private readonly ILogger<NewsBackgroundService> _logger;

        public NewsBackgroundService(NewsService newsService, ILogger<NewsBackgroundService> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Fetching news...");
                    await _newsService.GetNews();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching news.");
                }

                // Wait for 5 minutes before fetching news again
                await Task.Delay(TimeSpan.FromMinutes(20), stoppingToken);
            }
        }
    }
}
