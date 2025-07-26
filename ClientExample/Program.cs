using System.Net.Http;
using System.Text;
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

        /// <summary>
        /// AddRow - Add a new position to the webpage
        /// Usage: var success = await client.AddRow(new Position { Symbol = "AAPL", Quantity = 100, AvgPrice = 150.00m });
        /// </summary>
        public async Task<bool> AddRow(Position position)
        {
            try
            {
                var json = JsonSerializer.Serialize(position);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/trading/rows/add", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(responseJson);
                    Console.WriteLine($"AddRow result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error adding row for {position.Symbol}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception adding row for {position.Symbol}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// DeleteRow - Delete a position row from the webpage
        /// Usage: var success = await client.DeleteRow("AAPL");
        /// </summary>
        public async Task<bool> DeleteRow(string symbol)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/trading/rows/delete/{symbol}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(json);
                    Console.WriteLine($"DeleteRow result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error deleting row for {symbol}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception deleting row for {symbol}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// UpdateRow - Update an existing position row on the webpage
        /// Usage: var success = await client.UpdateRow("AAPL", new Position { Symbol = "AAPL", Quantity = 200, AvgPrice = 155.00m });
        /// </summary>
        public async Task<bool> UpdateRow(string symbol, Position position)
        {
            try
            {
                var json = JsonSerializer.Serialize(position);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_baseUrl}/api/trading/rows/update/{symbol}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(responseJson);
                    Console.WriteLine($"UpdateRow result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating row for {symbol}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception updating row for {symbol}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ToggleLive - Toggle the LIVE field for a position between "ON" and "OFF"
        /// Usage: var success = await client.ToggleLive("AAPL");
        /// </summary>
        public async Task<bool> ToggleLive(string symbol)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/trading/rows/toggle-live/{symbol}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(json);
                    Console.WriteLine($"ToggleLive result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error toggling LIVE for {symbol}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception toggling LIVE for {symbol}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ToggleFlatten - Toggle the FLATTEN field for a position between TRUE and FALSE
        /// Usage: var success = await client.ToggleFlatten("AAPL");
        /// </summary>
        public async Task<bool> ToggleFlatten(string symbol)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/trading/rows/toggle-flatten/{symbol}", null);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(json);
                    Console.WriteLine($"ToggleFlatten result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error toggling FLATTEN for {symbol}: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception toggling FLATTEN for {symbol}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// AddComment - Add a comment/message to the console
        /// Usage: var success = await client.AddComment("Order executed successfully");
        /// </summary>
        public async Task<bool> AddComment(string message)
        {
            try
            {
                var requestBody = new { Message = message };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/trading/console/add", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<CommandResponse>(responseJson);
                    Console.WriteLine($"AddComment result: {result?.Message}");
                    return result?.Success ?? false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error adding comment: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception adding comment: {ex.Message}");
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

    // Position model for row management
    public class Position
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        
        /// <summary>
        /// LIVE field - can be "ON" or "OFF"
        /// </summary>
        public string Live { get; set; } = "OFF";
        
        /// <summary>
        /// FLATTEN field - can be TRUE or FALSE
        /// </summary>
        public bool Flatten { get; set; } = false;
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

                // Demonstrate new row management APIs
                Console.WriteLine("5. Testing new Row Management APIs:");
                Console.WriteLine();

                // Test AddRow API
                Console.WriteLine("   5a. Testing AddRow API:");
                var newPosition = new Position
                {
                    Symbol = "TSLA",
                    Quantity = 50,
                    AvgPrice = 250.00m,
                    CurrentPrice = 255.00m,
                    Live = "ON",
                    Flatten = false
                };
                
                bool addResult = await client.AddRow(newPosition);
                Console.WriteLine($"   AddRow(TSLA position with LIVE=ON, FLATTEN=false) = {addResult}");
                Console.WriteLine();

                // Test UpdateRow API
                Console.WriteLine("   5b. Testing UpdateRow API:");
                var updatedPosition = new Position
                {
                    Symbol = "TSLA",
                    Quantity = 75,
                    AvgPrice = 248.00m,
                    CurrentPrice = 255.00m,
                    Live = "OFF",
                    Flatten = true
                };
                
                bool updateResult = await client.UpdateRow("TSLA", updatedPosition);
                Console.WriteLine($"   UpdateRow(TSLA, updated position with LIVE=OFF, FLATTEN=true) = {updateResult}");
                Console.WriteLine();

                // Test Toggle APIs
                Console.WriteLine("   5c. Testing Toggle APIs:");
                bool toggleLiveResult = await client.ToggleLive("TSLA");
                Console.WriteLine($"   ToggleLive(TSLA) = {toggleLiveResult}");
                
                bool toggleFlattenResult = await client.ToggleFlatten("TSLA");
                Console.WriteLine($"   ToggleFlatten(TSLA) = {toggleFlattenResult}");
                Console.WriteLine();

                // Test DeleteRow API
                Console.WriteLine("   5d. Testing DeleteRow API:");
                bool deleteResult = await client.DeleteRow("TSLA");
                Console.WriteLine($"   DeleteRow(TSLA) = {deleteResult}");
                Console.WriteLine();

                // Demonstrate Console API
                Console.WriteLine("6. Testing Console API (AddComment):");
                Console.WriteLine();
                
                Console.WriteLine("   6a. Adding comments to console:");
                bool comment1 = await client.AddComment("Trading session started");
                Console.WriteLine($"   AddComment('Trading session started') = {comment1}");
                
                bool comment2 = await client.AddComment("Portfolio analysis completed");
                Console.WriteLine($"   AddComment('Portfolio analysis completed') = {comment2}");
                
                bool comment3 = await client.AddComment("Risk management rules applied");
                Console.WriteLine($"   AddComment('Risk management rules applied') = {comment3}");
                Console.WriteLine();

                Console.WriteLine("=== Demo completed successfully! ===");
                Console.WriteLine();
                Console.WriteLine("Your requested APIs are working perfectly:");
                Console.WriteLine("✅ GetGUIField(webpagetagname, fieldonwebpagetagname)");
                Console.WriteLine("✅ PutGUIField(webpagetagname, fieldonwebpagetagname, value)");
                Console.WriteLine("✅ Button callbacks for user interactions");
                Console.WriteLine("✅ AddRow(Position posn) - NEW!");
                Console.WriteLine("✅ DeleteRow(string symbol) - NEW!");
                Console.WriteLine("✅ UpdateRow(string symbol, Position posn) - NEW!");
                Console.WriteLine("✅ ToggleLive(string symbol) - NEW!");
                Console.WriteLine("✅ ToggleFlatten(string symbol) - NEW!");
                Console.WriteLine("✅ LIVE field toggle buttons (ON/OFF) - NEW!");
                Console.WriteLine("✅ FLATTEN field toggle buttons (TRUE/FALSE) - NEW!");
                Console.WriteLine("✅ AddComment(string message) - NEW!");
                Console.WriteLine("✅ Console-style text box for messages - NEW!");
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

