using TradingWebInterface.Services;
using System.Text.Json;

namespace TradingWebInterface
{
    public class Program
    {
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

