using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StockApiApp.Hubs;
using StockApiApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;


namespace StockApiApp.Services
{
    public class NewsService
    {
        private readonly RedisService _redisService;
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ILogger<NewsService> _logger;

        private readonly HttpClient client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "x-rapidapi-key", "3f8ac7f65fmshf80d4fb0e86ca32p1dd83ajsn3eec19696e72" },
                { "x-rapidapi-host", "alpha-vantage.p.rapidapi.com" },
            }
        };

        public NewsService(RedisService redisService, IHubContext<StockHub> hubContext, ILogger<NewsService> logger)
        {
            _redisService = redisService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task GetNews()
        {
            HttpResponseMessage response = await client.GetAsync("https://alpha-vantage.p.rapidapi.com/query?function=NEWS_SENTIMENT&datatype=json");
            
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var parsedJson = JObject.Parse(content);

                await _redisService.ClearCacheAsync("latest_news");
                await _redisService.SetStringAsync("latest_news", parsedJson.ToString());

                _logger.LogInformation("News fetched and stored in Redis.");

                // Send to clients without altering the format
                await _hubContext.Clients.All.SendAsync("ReceiveNewsUpdate", content);
                _logger.LogInformation($"Sent news update to clients.");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
            
        }
    }
}
