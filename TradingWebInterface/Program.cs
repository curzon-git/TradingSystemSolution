using TradingWebInterface.Services;
using TradingWebInterface.Models;
using TradingWebInterface.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace TradingWebInterface
{
    public class Program
    {
        // Static reference to the SignalR hub context for external access
        private static IHubContext<TradingHub>? _hubContext;
        private static IServiceProvider? _serviceProvider;
        
        /// <summary>
        /// Refreshes the webpage with new position data using SignalR
        /// </summary>
        /// <param name="positions">Array of Position objects to update the webpage with</param>
        public static async Task RefreshWebPage(Position[] positions)
        {
            if (_hubContext == null)
            {
                Console.WriteLine("Warning: SignalR hub context not initialized. Cannot refresh webpage.");
                return;
            }

            try
            {
                Console.WriteLine($"RefreshWebPage called with {positions.Length} positions via SignalR");
                
                // Get the trading system service and update positions
                var serviceProvider = _serviceProvider;
                if (serviceProvider != null)
                {
                    var tradingService = serviceProvider.GetRequiredService<ITradingSystemService>();
                    await tradingService.UpdatePositionsAsync(positions);
                    
                    // Get updated account summary and send to all clients
                    var accountSummary = await tradingService.GetAccountSummaryAsync();
                    await _hubContext.Clients.All.SendAsync("PositionsUpdate", accountSummary);
                    
                    Console.WriteLine("Webpage positions updated successfully via SignalR");
                }
                else
                {
                    Console.WriteLine("Warning: Service provider not available.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing webpage via SignalR: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current positions from the webpage using SignalR
        /// Note: This function gets data directly from the backend service
        /// </summary>
        /// <returns>Array of Position objects currently displayed on the webpage</returns>
        public static async Task<Position[]> GetWebPage()
        {
            if (_serviceProvider == null)
            {
                Console.WriteLine("Warning: Service provider not initialized. Cannot get webpage data.");
                return Array.Empty<Position>();
            }

            try
            {
                Console.WriteLine("GetWebPage called via SignalR");
                
                // Get current positions from the backend service
                var tradingService = _serviceProvider.GetRequiredService<ITradingSystemService>();
                var accountSummary = await tradingService.GetAccountSummaryAsync();
                var positions = accountSummary.Positions.Values.ToArray();
                
                Console.WriteLine($"GetWebPage returning {positions.Length} positions");
                
                return positions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting webpage data via SignalR: {ex.Message}");
                return Array.Empty<Position>();
            }
        }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Register your trading system service
            builder.Services.AddSingleton<ITradingSystemService, TradingSystemService>();
            builder.Services.AddSingleton<ScreenFieldManager>();
            builder.Services.AddSingleton<IConsoleService, ConsoleService>();
            builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();
            builder.Services.AddSingleton<IWebPageInterfaceService, WebPageInterfaceService>();
            
            // Add SignalR
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Initialize static SignalR hub context for external access
            _hubContext = app.Services.GetRequiredService<IHubContext<TradingHub>>();
            _serviceProvider = app.Services;

            // Configure the HTTP request pipeline
            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers();
            
            // Map SignalR hub
            app.MapHub<TradingHub>("/tradingHub");

            // Serve the main page
            app.MapFallbackToFile("index.html");

            // Configure URLs for deployment
            // Multiple URL bindings for flexibility
            app.Urls.Clear(); // Clear default URLs

            // Primary deployment URL
            //app.Urls.Add("http://213.123.220.183:5000");

            // Fallback URLs for local access and testing
            app.Urls.Add("http://0.0.0.0:5000");
            app.Urls.Add("http://localhost:5000");

            Console.WriteLine("=== C# Trading Interface Starting ===");
            Console.WriteLine($"Primary URL: http://213.123.220.183:5000");
            Console.WriteLine($"Local URL: http://localhost:5000");
            Console.WriteLine($"Network URL: http://0.0.0.0:5000");
            Console.WriteLine("Press Ctrl+C to stop");

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                Console.WriteLine("Common issues:");
                Console.WriteLine("1. IP address 213.123.220.183 not assigned to this server");
                Console.WriteLine("2. Port 5000 already in use");
                Console.WriteLine("3. Insufficient permissions");
                Console.WriteLine("4. Firewall blocking the port");
                throw;
            }
        }
    }
}

