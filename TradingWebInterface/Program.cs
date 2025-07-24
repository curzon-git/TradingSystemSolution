using TradingWebInterface.Services;

namespace TradingWebInterface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Register your trading system services
            builder.Services.AddSingleton<ITradingSystemService, TradingSystemService>();
            builder.Services.AddSingleton<ScreenFieldManager>();

            // Add logging
            builder.Services.AddLogging();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllers();

            // Serve the main page for any unmatched routes
            app.MapFallbackToFile("index.html");

            // Configure to listen on all interfaces for remote access
            app.Urls.Add("http://0.0.0.0:5000");

            Console.WriteLine("=== C# Trading Web Interface Starting ===");
            Console.WriteLine("Local access: http://localhost:5000");
            Console.WriteLine("Remote access: http://[your-server-ip]:5000");
            Console.WriteLine("Press Ctrl+C to stop the server");
            Console.WriteLine();

            app.Run();
        }
    }
}

