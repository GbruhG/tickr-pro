namespace StockApiApp.Models
{
    public class CryptoTrade
    {
        public string Ticker { get; set; }  // e.g., "BINANCE:BTCUSDT"
        public decimal Price { get; set; }  // Last traded price
        public decimal Volume { get; set; } // Trade volume
        public decimal VWAP { get; set; }   // Volume-weighted average price
        public long Timestamp { get; set; } // Trade timestamp (Unix time)

        public CryptoTrade(string ticker, decimal price, decimal volume, decimal vwap, long timestamp)
        {
            Ticker = ticker;
            Price = price;
            Volume = volume;
            VWAP = vwap;
            Timestamp = timestamp;
        }
    }
}