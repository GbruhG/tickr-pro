using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using StockApiApp.Hubs;
using StockApiApp.Models;

namespace StockApiApp.Services
{
    public class WebSocketService : BackgroundService
    {
        private ClientWebSocket _webSocket;
        private readonly string _webSocketUrl = "wss://ws.finnhub.io?token=cv8kn39r01qqdqh6m0k0cv8kn39r01qqdqh6m0kg";
        private readonly IHubContext<StockHub> _hubContext;
        private readonly ILogger<WebSocketService> _logger;
        private CancellationTokenSource _heartbeatCts;
        private readonly object _lock = new object();

        public WebSocketService(IHubContext<StockHub> hubContext, ILogger<WebSocketService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int retryCount = 0;
            const int maxRetries = 5; // Max retry attempts
            int baseDelay = 5000; // Base delay (5 seconds)

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Attempt to connect and listen
                    await ConnectAndListenAsync(stoppingToken);
                    retryCount = 0; // Reset retry count on successful connection
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WebSocket connection");

                    // Clean up the old WebSocket
                    CleanupWebSocket();

                    // Exponential backoff with a maximum retry delay
                    int delay = Math.Min(baseDelay * (int)Math.Pow(2, retryCount), 30000); // Max 30 seconds
                    _logger.LogInformation($"Reconnection attempt #{retryCount + 1}. Waiting for {delay}ms before retrying.");

                    // Wait before reconnecting
                    await Task.Delay(delay, stoppingToken);

                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError("Max retries reached. Giving up on WebSocket connection.");
                        break; // Optionally stop after max retries
                    }
                }
            }
        }


        private void CleanupWebSocket()
        {
            lock (_lock)
            {
                if (_webSocket != null)
                {
                    try
                    {
                        // Cancel heartbeat if it's still active
                        _heartbeatCts?.Cancel();
                        _heartbeatCts?.Dispose();
                        _heartbeatCts = null;

                        // Close the WebSocket if it's still open
                        if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.Connecting)
                        {
                            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cleaning up", CancellationToken.None)
                                .Wait(1000); // Wait up to 1 second for close
                        }

                        _webSocket.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cleaning up WebSocket");
                    }
                    finally
                    {
                        _webSocket = null;
                    }
                }
            }
        }


        private async Task ConnectAndListenAsync(CancellationToken stoppingToken)
        {
            // Create a new WebSocket instance
            lock (_lock)
            {
                if (_webSocket != null)
                {
                    CleanupWebSocket();
                }
                _webSocket = new ClientWebSocket();
            }
            
            _logger.LogInformation("Connecting to WebSocket...");
            await _webSocket.ConnectAsync(new Uri(_webSocketUrl), stoppingToken);
            _logger.LogInformation("Connected to WebSocket!");
            
            // Start heartbeat
            _heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            _ = StartHeartbeatAsync(_heartbeatCts.Token);
            
            await OnOpenAsync(stoppingToken);
            await ListenAsync(stoppingToken);
        }

        private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(30000, cancellationToken); // Send ping every 30 seconds
                    
                    if (_webSocket?.State == WebSocketState.Open)
                    {
                        await SendMessageAsync("{\"type\":\"ping\"}", cancellationToken);
                        _logger.LogDebug("Sent ping to Finnhub");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, do nothing
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in heartbeat task");
            }
        }

        private async Task OnOpenAsync(CancellationToken stoppingToken)
        {
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"AAPL\"}", stoppingToken);  // Apple
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"MSFT\"}", stoppingToken);  // Microsoft
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"GOOGL\"}", stoppingToken); // Alphabet (Google)
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"AMZN\"}", stoppingToken);  // Amazon
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"META\"}", stoppingToken);  // Meta (Facebook)
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"TSLA\"}", stoppingToken);  // Tesla
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"NVDA\"}", stoppingToken);  // NVIDIA
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"JPM\"}", stoppingToken);   // JPMorgan Chase
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"V\"}", stoppingToken);     // Visa
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"WMT\"}", stoppingToken);   // Walmart
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"JNJ\"}", stoppingToken);   // Johnson & Johnson
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"PG\"}", stoppingToken);    // Procter & Gamble
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"DIS\"}", stoppingToken);   // Disney
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"KO\"}", stoppingToken);    // Coca-Cola
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"AMD\"}", stoppingToken);   // Advanced Micro Devices

            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:BTCUSDT\"}", stoppingToken);  // Bitcoin
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:ETHUSDT\"}", stoppingToken);  // Ethereum
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:SOLUSDT\"}", stoppingToken);  // Solana
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:BNBUSDT\"}", stoppingToken);  // Binance Coin
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:ADAUSDT\"}", stoppingToken);  // Cardano
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:XRPUSDT\"}", stoppingToken);  // XRP (Ripple)
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:DOGEUSDT\"}", stoppingToken); // Dogecoin
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:DOTUSDT\"}", stoppingToken);  // Polkadot
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:AVAXUSDT\"}", stoppingToken); // Avalanche
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:MATICUSDT\"}", stoppingToken); // Polygon
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:LINKUSDT\"}", stoppingToken); // Chainlink
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:UNIUSDT\"}", stoppingToken);  // Uniswap
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:LTCUSDT\"}", stoppingToken);  // Litecoin
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:ATOMUSDT\"}", stoppingToken); // Cosmos
            await SendMessageAsync("{\"type\":\"subscribe\",\"symbol\":\"BINANCE:SHIBUSDT\"}", stoppingToken); // Shiba Inu
        }

        private async Task SendMessageAsync(string message, CancellationToken stoppingToken)
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning($"Cannot send message, WebSocket is not open. State: {_webSocket?.State}");
                return;
            }
            
            var buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, stoppingToken);
            _logger.LogInformation($"Sent: {message}");
        }

        private async Task ListenAsync(CancellationToken stoppingToken)
        {
            var buffer = new byte[8192];

            while (_webSocket?.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket closed.");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", stoppingToken);
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogDebug($"Received: {message}");

                if (!string.IsNullOrWhiteSpace(message))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(message);
                        var root = doc.RootElement;

                        // Check if this is a ping response
                        if (root.TryGetProperty("type", out var typeElement) && typeElement.GetString() == "pong")
                        {
                            _logger.LogDebug("Received pong from Finnhub");
                            continue;
                        }

                        if (root.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var tradeElement in dataElement.EnumerateArray())
                            {
                                string ticker = tradeElement.GetProperty("s").GetString();
                                decimal price = tradeElement.GetProperty("p").GetDecimal();
                                decimal volume = tradeElement.GetProperty("v").GetDecimal();
                                long timestamp = tradeElement.GetProperty("t").GetInt64();

                                if (ticker.StartsWith("BINANCE:"))
                                {
                                    var cryptoTrade = new CryptoTrade(ticker, price, volume, price, timestamp);
                                    await _hubContext.Clients.All.SendAsync("ReceiveCryptoUpdate", cryptoTrade, stoppingToken);
                                }
                                else 
                                {
                                    var stockTrade = new StockTrade(ticker, price, volume, price, timestamp);
                                    await _hubContext.Clients.All.SendAsync("ReceiveTradeUpdate", stockTrade, stoppingToken);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing message");
                    }
                }
            }
            
            // Stop heartbeat when connection closes
            _heartbeatCts?.Cancel();
        }
    }
}