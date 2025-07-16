using Microsoft.AspNetCore.Mvc;
using StockApiApp.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


namespace StockApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsService _newsService;
        private readonly RedisService _redisService;
        private readonly HttpClient client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "x-rapidapi-key", "" },
                { "x-rapidapi-host", "alpha-vantage.p.rapidapi.com" },
            }
        };
        

        public NewsController(NewsService newsService, RedisService redisService)
        {
            _newsService = newsService;
            _redisService = redisService;
        }

        [HttpGet("get-latest-news")]
        public async Task<IActionResult> GetLatestNews()
        {
            // Try to get news from Redis cache
            var cachedNews = await _redisService.GetStringAsync("latest_news");

            if (!string.IsNullOrEmpty(cachedNews))
            {
                // Deserialize once to remove double-encoded JSON
                var cleanedNews = JsonSerializer.Deserialize<JsonElement>(cachedNews);
                return Ok(new { News = cleanedNews, FromCache = true });
            }

            // If no cached news, fetch and cache the news
            await _newsService.GetNews();

            // Retrieve the fresh news
            var freshNews = await _redisService.GetStringAsync("latest_news");

            if (string.IsNullOrEmpty(freshNews))
            {
                return BadRequest(new { Message = "No news available", FromCache = false });
            }

            // Deserialize to remove escape characters
            var cleanedFreshNews = JsonSerializer.Deserialize<JsonElement>(freshNews);

            return Ok(new { News = cleanedFreshNews, FromCache = false });
        }


        // private async Task<HttpResponseMessage> FetchAndCacheNews()
        // {
        //     // Fetch the news from the external API
        //     HttpResponseMessage response = await client.GetAsync("https://alpha-vantage.p.rapidapi.com/query?function=NEWS_SENTIMENT&datatype=json");

        //     if (response.IsSuccessStatusCode)
        //     {
        //         // Read the news and cache it in Redis for 10 minutes
        //         string content = await response.Content.ReadAsStringAsync();
        //         await _redisService.SetStringAsync("latest_news", content, TimeSpan.FromMinutes(10));
        //     }

        //     return response;
        // }
    }
}
