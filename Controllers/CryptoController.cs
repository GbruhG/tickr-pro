using Microsoft.AspNetCore.Mvc;
using StockApiApp.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


namespace StockApiApp.Controllers
{
    [Route("api/crypto")]
    [ApiController]
    public class CryptoController : ControllerBase
    {
        private readonly CryptoService _cryptoService;
        private readonly RedisService _redisService;
        
        public CryptoController(CryptoService cryptoService, RedisService redisService)
        {
            _cryptoService = cryptoService;
            _redisService = redisService;
        }

        [HttpGet("trending")]
        public async Task<IActionResult> GetLatestNews()
        {
            // Try to get news from Redis cache
            var cachedCrypto = await _redisService.GetStringAsync("trending_crypto");

            if (!string.IsNullOrEmpty(cachedCrypto))
            {
                // Deserialize once to remove double-encoded JSON
                var cleanedCrypto = JsonSerializer.Deserialize<JsonElement>(cachedCrypto);
                return Ok(new { Crypto = cleanedCrypto, FromCache = true });
            }

            // If no cached news, fetch and cache the news
            await _cryptoService.GetTrendingCrypto();

            // Retrieve the fresh news
            var freshCryptoTrending = await _redisService.GetStringAsync("trending_crypto");

            // Deserialize to remove escape characters
            var cleanedFreshCrypto = JsonSerializer.Deserialize<JsonElement>(freshCryptoTrending);

            return Ok(new { News = cleanedFreshCrypto, FromCache = false });
        }

        
    }
}
