using TradingWebInterface.Services;
using TradingWebInterface.Models;
using System.Text.Json;

namespace TradingWebInterface
{
    public class Program
    {
        // Static reference to the trading system service for external access
        private static ITradingSystemService? _tradingSystemService;
        
        /// <summary>
        /// Refreshes the webpage with new position data
        /// </summary>
        /// <param name="positions">Array of Position objects to update the webpage with</param>
        public static async Task RefreshWebPage(Position[] positions)
        {
            if (_tradingSystemService == null)
            {
                Console.WriteLine("Warning: Trading system service not initialized. Cannot refresh webpage.");
                return;
            }

            try
            {
                Console.WriteLine($"RefreshWebPage called with {positions.Length} positions");
                
                // Update the positions in the trading system service
                await _tradingSystemService.UpdatePositionsAsync(positions);
                
                Console.WriteLine("Webpage positions updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing webpage: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current positions from the webpage
        /// </summary>
        /// <returns>Array of Position objects currently displayed on the webpage</returns>
        public static async Task<Position[]> GetWebPage()
        {
            if (_tradingSystemService == null)
            {
                Console.WriteLine("Warning: Trading system service not initialized. Cannot get webpage data.");
                return Array.Empty<Position>();
            }

            try
            {
                Console.WriteLine("GetWebPage called");
                
                // Get current account summary which contains positions
                var accountSummary = await _tradingSystemService.GetAccountSummaryAsync();
                var positionsArray = accountSummary.Positions.Values.ToArray();
                
                Console.WriteLine($"Retrieved {positionsArray.Length} positions from webpage");
                
                return positionsArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting webpage data: {ex.Message}");
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

            var app = builder.Build();

            // Initialize static service reference for external access
            _tradingSystemService = app.Services.GetRequiredService<ITradingSystemService>();

            // Configure the HTTP request pipeline
            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers();

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

