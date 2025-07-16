using System.Text.Json;
using System.Net.Http;

public class StockService
{
    private readonly List<Stock> _stocks;
    private readonly HttpClient _httpClient;

    public StockService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _stocks = LoadStocks();
    }

    private List<Stock> LoadStocks()
    {
        var json = File.ReadAllText("Data/stocks.json");
        var stocks = JsonSerializer.Deserialize<List<Stock>>(json);
        return JsonSerializer.Deserialize<List<Stock>>(json) ?? new();
        
    }

    public List<Stock> SearchStocks(string query) =>
        _stocks
            .Where(s => s.Ticker.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        s.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

    public async Task<object> GetStockDetailsAsync(string ticker)
    {
        // Example: Fetch live price from Alpha Vantage
        var response = await _httpClient.GetStringAsync($"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={ticker}&apikey=YOUR_API_KEY");
        return JsonSerializer.Deserialize<object>(response);
    }
}

public class Stock : IAsset
{
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
