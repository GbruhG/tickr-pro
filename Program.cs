using StockApiApp.Hubs;
using StockApiApp.Services;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .SetIsOriginAllowed(origin => true) // Allow any origin for development
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for SignalR
    });
});

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddSingleton<RedisService>();
builder.Services.AddSingleton<NewsService>();

// Register WebSocketService as a hosted service
builder.Services.AddHostedService<WebSocketService>();
// Register news service as service, it runs every 5 minutes
builder.Services.AddHostedService<NewsBackgroundService>();
builder.Services.AddHostedService<CryptoTrendingService>();

builder.Services.AddHttpClient<StockService>();
builder.Services.AddHttpClient<CryptoService>();
builder.Services.AddScoped<StockService>(); 
builder.Services.AddScoped<CryptoService>();




var app = builder.Build(); 

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    // Comment out the HTTPS redirection for development
    // app.UseHttpsRedirection();
}

// USE CORS BEFORE ROUTING - This order is important!
app.UseCors("CorsPolicy");

app.UseRouting();
app.UseAuthorization();

// Add a health check endpoint to verify the server is running
app.MapGet("/health", () => "API is running");

// Map SignalR hub
app.MapHub<StockHub>("/stockHub");
app.MapControllers();

app.Run();