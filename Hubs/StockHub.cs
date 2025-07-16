using Microsoft.AspNetCore.SignalR;
using StockApiApp.Models;

namespace StockApiApp.Hubs
{
    public class StockHub : Hub
    {
        public async Task SendTradeUpdate(StockTrade trade)
        {
            await Clients.All.SendAsync("ReceiveTradeUpdate", trade);
        }

        public async Task SendCryptoUpdate(StockTrade trade)
        {
            await Clients.All.SendAsync("ReceiveCryptoUpdate", trade);
        }

        public async Task SendNewsUpdate(News news)
        {
            await Clients.All.SendAsync("ReceiveNewsUpdate", news);
        }
    }
}