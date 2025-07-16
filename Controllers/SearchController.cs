using Microsoft.AspNetCore.Mvc;
using StockApiApp.Services;

public interface IAsset
{
    string Ticker { get; set; }
    string Name { get; set; }
}

[ApiController]
[Route("api/ticker")]
public class SearchController : ControllerBase
{
    private readonly StockService _stockService;
    private readonly CryptoService _cryptoService;

    public SearchController(StockService stockService, CryptoService cryptoService)
    {
        _stockService = stockService;
        _cryptoService = cryptoService;
    }

    [HttpGet("search")]
    public IActionResult Search(string query)
    {
        // Search through stocks and cryptos
        var stockResults = _stockService.SearchStocks(query);
        var cryptoResults = _cryptoService.SearchCryptos(query);

        // Combine results and convert to a common type (IEnumerable<object> or IEnumerable<ISearchResult>)
        var combinedResults = stockResults
            .Cast<object>()
            .Concat(cryptoResults.Cast<object>())
            .Take(10)
            .ToList();

        return Ok(combinedResults);
    }


    [HttpGet("details")]
    public async Task<IActionResult> GetDetails([FromQuery] string ticker, [FromQuery] string type)
    {
        if (string.IsNullOrWhiteSpace(ticker)) return BadRequest("Ticker is required");

        var details = type switch
        {
            "stock" => await _stockService.GetStockDetailsAsync(ticker),
            "crypto" => await _cryptoService.GetCryptoDetailsAsync(ticker),
            _ => null
        };

        return details == null ? NotFound() : Ok(details);
    }
}