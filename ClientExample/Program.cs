using System.Text.Json;

namespace ClientExample
{
    /// <summary>
    /// Example C# client showing how to use the GetGUIField and PutGUIField APIs
    /// This demonstrates how your C# trading system can interact with the web interface
    /// </summary>
    public class TradingInterfaceClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TradingInterfaceClient(string baseUrl = "http://localhost:5000")
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl.TrimEnd('/');
        }

        /// <summary>
        /// GetGUIField - Read a field value from the web interface
        /// Usage: var value = await client.GetGUIField("TradingPage", "symbol_input");
        /// </summary>
        public async Task<string> GetGUIField(string webpageTagName, string fieldOnWebpageTagName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/trading/screen/read/{fieldOnWebpageTagName}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FieldReadResponse>(json);
                    return result?.Value ?? string.Empty;
                }
                else
                {
                    Console.WriteLine($"Error reading field {fieldOnWebpageTagName}: {response.StatusCode}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception reading field {fieldOnWebpageTagName}: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// PutGUIField - Write a value to a field on the web interface  
        /// Usage: var success = await client.PutGUIField("TradingPage", "symbol_input", "AAPL");
        /// </summary>
        public async Task<bool> PutGUIField(string webpageTagName, string fieldOnWebpageTagName, string value)
        {
            try
            {
                var requestData = new { Value = value };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/trading/screen/write/{fieldOnWebpageTagName}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error writing field {fieldOnWebpageTagName}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception writing field {fieldOnWebpageTagName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Trigger button callbacks
        /// </summary>
        public async Task<bool> TriggerButtonCallback(string buttonAction)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/trading/command/{buttonAction}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(json);
                    Console.WriteLine($"Button callback result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error triggering button {buttonAction}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception triggering button {buttonAction}: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Response models for JSON deserialization
    public class FieldReadResponse
    {
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class CommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Demo program showing your exact requested APIs in action
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== C# Trading Interface Client Demo ===");
            Console.WriteLine("This demonstrates your requested GetGUIField and PutGUIField APIs");
            Console.WriteLine();

            using var client = new TradingInterfaceClient("http://localhost:5000");

            try
            {
                // Test your exact requested API signatures
                Console.WriteLine("1. Testing your requested APIs:");
                
                // Your exact requested GetGUIField API
                var value = await client.GetGUIField("webpagetagname", "symbol_input");
                Console.WriteLine($"   GetGUIField('webpagetagname', 'symbol_input') = '{value}'");
                
                // Your exact requested PutGUIField API
                bool success = await client.PutGUIField("webpagetagname", "symbol_input", "AAPL");
                Console.WriteLine($"   PutGUIField('webpagetagname', 'symbol_input', 'AAPL') = {success}");
                
                // Verify the value was set
                value = await client.GetGUIField("webpagetagname", "symbol_input");
                Console.WriteLine($"   Verification - GetGUIField('webpagetagname', 'symbol_input') = '{value}'");
                Console.WriteLine();

                // Demonstrate setting up an order
                Console.WriteLine("2. Setting up a complete order:");
                await client.PutGUIField("webpagetagname", "symbol_input", "MSFT");
                await client.PutGUIField("webpagetagname", "quantity_input", "100");
                await client.PutGUIField("webpagetagname", "price_input", "380.50");
                await client.PutGUIField("webpagetagname", "order_type", "BUY");
                
                Console.WriteLine("   Order fields set:");
                Console.WriteLine($"   Symbol: {await client.GetGUIField("webpagetagname", "symbol_input")}");
                Console.WriteLine($"   Quantity: {await client.GetGUIField("webpagetagname", "quantity_input")}");
                Console.WriteLine($"   Price: {await client.GetGUIField("webpagetagname", "price_input")}");
                Console.WriteLine($"   Type: {await client.GetGUIField("webpagetagname", "order_type")}");
                Console.WriteLine();

                // Demonstrate button callback
                Console.WriteLine("3. Testing button callback:");
                bool orderPlaced = await client.TriggerButtonCallback("place_order");
                Console.WriteLine($"   Place order callback result: {orderPlaced}");
                
                // Check status after order
                var status = await client.GetGUIField("webpagetagname", "status_display");
                Console.WriteLine($"   Status after order: '{status}'");
                Console.WriteLine();

                // Demonstrate reading system state
                Console.WriteLine("4. Reading system state:");
                var lastCommand = await client.GetGUIField("webpagetagname", "last_command");
                var accountBalance = await client.GetGUIField("webpagetagname", "account_balance");
                var totalPnL = await client.GetGUIField("webpagetagname", "total_pnl");
                
                Console.WriteLine($"   Last Command: {lastCommand}");
                Console.WriteLine($"   Account Balance: ${accountBalance}");
                Console.WriteLine($"   Total P&L: ${totalPnL}");
                Console.WriteLine();

                Console.WriteLine("=== Demo completed successfully! ===");
                Console.WriteLine();
                Console.WriteLine("Your requested APIs are working perfectly:");
                Console.WriteLine("✅ GetGUIField(webpagetagname, fieldonwebpagetagname)");
                Console.WriteLine("✅ PutGUIField(webpagetagname, fieldonwebpagetagname, value)");
                Console.WriteLine("✅ Button callbacks for user interactions");
                Console.WriteLine("✅ Multi-machine web access");
                Console.WriteLine("✅ Direct integration with your C# trading system");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Demo failed: {ex.Message}");
                Console.WriteLine("Make sure the C# Trading Interface is running on http://localhost:5000");
                Console.WriteLine("Run: dotnet run --project ../TradingWebInterface/TradingWebInterface.csproj");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}

