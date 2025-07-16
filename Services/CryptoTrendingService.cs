using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockApiApp.Services
{
    public class CryptoTrendingService : BackgroundService
    {
        private readonly CryptoService _cryptoService;
        private readonly ILogger<NewsBackgroundService> _logger;

        public CryptoTrendingService(CryptoService cryptoService, ILogger<NewsBackgroundService> logger)
        {
            _cryptoService = cryptoService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Fetching crypto..");
                    await _cryptoService.GetTrendingCrypto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while fetching crypto.");
                }

                // Wait for 5 minutes before fetching crypto again
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }
    }
}
