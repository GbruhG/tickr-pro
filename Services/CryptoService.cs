using System.Text.Json;
using System.Net.Http;
using StockApiApp.Hubs;
using Newtonsoft.Json.Linq;


namespace StockApiApp.Services
{
    public class CryptoService
    {
        private readonly List<Crypto> _cryptos;
        private readonly HttpClient client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "x-cg-pro-api-key", "CG-JbZcAi1C4GvUwfswrMh6xSaA" }
            }
        };
        private readonly RedisService _redisService;
        private readonly ILogger<CryptoService> _logger;
        public CryptoService(RedisService redisService, ILogger<CryptoService> logger)
        {
            _cryptos = LoadCryptos();
            _redisService = redisService;
            _logger = logger;
        }

        private List<Crypto> LoadCryptos()
        {
            var json = File.ReadAllText("Data/crypto.json");
            return JsonSerializer.Deserialize<List<Crypto>>(json) ?? new();
        }

        public List<Crypto> SearchCryptos(string query) =>
            _cryptos
                .Where(c => c.Ticker.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();


        //not working, fix later
        public async Task<object> GetCryptoDetailsAsync(string ticker)
        {
            // Example: Fetch price from CoinGecko
            var response = await client.GetStringAsync($"https://api.coingecko.com/api/v3/simple/price?ids={ticker}&vs_currencies=usd");
            return JsonSerializer.Deserialize<object>(response);
        }

        public async Task GetTrendingCrypto(){
            HttpResponseMessage response = await client.GetAsync("https://api.coingecko.com/api/v3/search/trending");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var parsedJson = JObject.Parse(content);

                await _redisService.ClearCacheAsync("trending_crypto");
                await _redisService.SetStringAsync("trending_crypto", parsedJson.ToString());

                _logger.LogInformation("Trending crypto fetched and stored in Redis.");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
    }

    public class Crypto : IAsset
    {
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}