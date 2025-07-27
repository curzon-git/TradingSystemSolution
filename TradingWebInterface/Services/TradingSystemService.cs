using TradingWebInterface.Models;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Implementation of trading system service
    /// Replace the mock data with calls to your actual C# trading system
    /// </summary>
    public class TradingSystemService : ITradingSystemService
    {
        private readonly Dictionary<string, Position> _positions;
        private decimal _accountBalance;
        private readonly Random _random;

        public TradingSystemService()
        {
            _random = new Random();
            _accountBalance = 50000.00m;
            
            // Mock positions - replace with calls to your trading system
            _positions = new Dictionary<string, Position>
            {
                ["AAPL"] = new Position 
                { 
                    Symbol = "AAPL", 
                    Quantity = 100, 
                    AvgPrice = 150.25m, 
                    CurrentPrice = 155.30m 
                },
                ["GOOGL"] = new Position 
                { 
                    Symbol = "GOOGL", 
                    Quantity = -50, 
                    AvgPrice = 2800.00m, 
                    CurrentPrice = 2795.50m 
                },
                ["MSFT"] = new Position 
                { 
                    Symbol = "MSFT", 
                    Quantity = 200, 
                    AvgPrice = 380.75m, 
                    CurrentPrice = 385.20m 
                }
            };

            // Start background price simulation
            _ = Task.Run(SimulatePriceMovements);
        }

        public async Task<AccountSummary> GetAccountSummaryAsync()
        {
            // TODO: Replace with call to your trading system
            // Example: return await YourTradingSystem.GetAccountSummaryAsync();
            
            await Task.Delay(10); // Simulate async call
            
            var totalPnL = _positions.Values.Sum(p => p.PnL);
            
            return new AccountSummary
            {
                AccountBalance = _accountBalance,
                TotalPnL = totalPnL,
                Positions = new Dictionary<string, Position>(_positions),
                LastUpdate = DateTime.Now
            };
        }

        public async Task<Dictionary<string, Position>> GetPositionsAsync()
        {
            // TODO: Replace with call to your trading system
            // Example: return await YourTradingSystem.GetPositionsAsync();
            
            await Task.Delay(10); // Simulate async call
            return new Dictionary<string, Position>(_positions);
        }

        public async Task<OrderResult> PlaceOrderAsync(OrderRequest order)
        {
            // TODO: Replace with call to your trading system
            // Example: return await YourTradingSystem.PlaceOrderAsync(order);
            
            await Task.Delay(100); // Simulate order processing time
            
            try
            {
                // Validate order
                if (string.IsNullOrEmpty(order.Symbol))
                    throw new ArgumentException("Symbol is required");
                
                if (order.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0");
                
                if (order.Price <= 0)
                    throw new ArgumentException("Price must be greater than 0");

                // Adjust quantity for sell orders
                var quantity = order.OrderType.ToUpper() == "SELL" ? -order.Quantity : order.Quantity;
                
                // Update positions (simplified logic)
                if (_positions.ContainsKey(order.Symbol))
                {
                    var existingPos = _positions[order.Symbol];
                    var newQuantity = existingPos.Quantity + quantity;
                    
                    if (newQuantity == 0)
                    {
                        // Position closed
                        _positions.Remove(order.Symbol);
                    }
                    else
                    {
                        // Update position
                        var totalCost = (existingPos.Quantity * existingPos.AvgPrice) + (quantity * order.Price);
                        _positions[order.Symbol] = new Position
                        {
                            Symbol = order.Symbol,
                            Quantity = newQuantity,
                            AvgPrice = Math.Abs(totalCost / newQuantity),
                            CurrentPrice = order.Price,
                            Live = existingPos.Live,
                            Flatten = existingPos.Flatten
                        };
                    }
                }
                else
                {
                    // New position
                    _positions[order.Symbol] = new Position
                    {
                        Symbol = order.Symbol,
                        Quantity = quantity,
                        AvgPrice = order.Price,
                        CurrentPrice = order.Price
                    };
                }

                var orderId = Guid.NewGuid().ToString("N")[..8].ToUpper();
                
                return new OrderResult
                {
                    Success = true,
                    Message = $"Order executed: {order.OrderType} {order.Quantity} {order.Symbol} @ ${order.Price:F2}",
                    OrderId = orderId
                };
            }
            catch (Exception ex)
            {
                return new OrderResult
                {
                    Success = false,
                    Message = $"Order failed: {ex.Message}",
                    OrderId = string.Empty
                };
            }
        }

        public async Task<decimal> GetCurrentPriceAsync(string symbol)
        {
            // TODO: Replace with call to your trading system or market data provider
            // Example: return await YourMarketDataProvider.GetPriceAsync(symbol);
            
            await Task.Delay(50); // Simulate market data call
            
            if (_positions.ContainsKey(symbol))
            {
                return _positions[symbol].CurrentPrice;
            }
            
            // Return a mock price for unknown symbols
            return 100.00m + (decimal)(_random.NextDouble() * 100);
        }

        public async Task<bool> IsConnectedAsync()
        {
            // TODO: Replace with actual connection check to your trading system
            // Example: return await YourTradingSystem.IsConnectedAsync();
            
            await Task.Delay(10);
            return true; // Mock: always connected
        }

        public async Task<string> GetSystemStatusAsync()
        {
            // TODO: Replace with actual system status from your trading system
            // Example: return await YourTradingSystem.GetStatusAsync();
            
            await Task.Delay(10);
            var isConnected = await IsConnectedAsync();
            return isConnected ? "Connected and Ready" : "Disconnected";
        }

        /// <summary>
        /// Add a new position to the system
        /// </summary>
        public async Task<OrderResult> AddPositionAsync(Position position)
        {
            // TODO: Replace with call to your trading system
            // Example: return await YourTradingSystem.AddPositionAsync(position);
            
            await Task.Delay(50); // Simulate processing time
            
            try
            {
                // Validate position data
                if (string.IsNullOrEmpty(position.Symbol))
                    throw new ArgumentException("Symbol is required");
                
                if (position.Quantity == 0)
                    throw new ArgumentException("Quantity cannot be zero");
                
                if (position.AvgPrice <= 0)
                    throw new ArgumentException("Average price must be greater than zero");

                // Check if position already exists
                if (_positions.ContainsKey(position.Symbol))
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Position for {position.Symbol} already exists. Use UpdateRow to modify it.",
                        OrderId = string.Empty
                    };
                }

                // Set current price if not provided
                if (position.CurrentPrice <= 0)
                {
                    position.CurrentPrice = position.AvgPrice;
                }

                // Add the new position
                _positions[position.Symbol] = new Position
                {
                    Symbol = position.Symbol,
                    Quantity = position.Quantity,
                    AvgPrice = position.AvgPrice,
                    CurrentPrice = position.CurrentPrice
                };

                return new OrderResult
                {
                    Success = true,
                    Message = $"Position added successfully: {position.Symbol} ({position.Quantity} shares @ ${position.AvgPrice:F2})",
                    OrderId = Guid.NewGuid().ToString("N")[..8].ToUpper()
                };
            }
            catch (Exception ex)
            {
                return new OrderResult
                {
                    Success = false,
                    Message = $"Failed to add position: {ex.Message}",
                    OrderId = string.Empty
                };
            }
        }

        /// <summary>
        /// Delete a position from the system
        /// </summary>
        public async Task<OrderResult> DeletePositionAsync(string symbol)
        {
            // TODO: Replace with call to your trading system
            // Example: return await YourTradingSystem.DeletePositionAsync(symbol);
            
            await Task.Delay(50); // Simulate processing time
            
            try
            {
                if (string.IsNullOrEmpty(symbol))
                    throw new ArgumentException("Symbol is required");

                // Check if position exists
                if (!_positions.ContainsKey(symbol))
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Position for {symbol} not found",
                        OrderId = string.Empty
                    };
                }

                // Remove the position
                var removedPosition = _positions[symbol];
                _positions.Remove(symbol);

                return new OrderResult
                {
                    Success = true,
                    Message = $"Position deleted successfully: {symbol} ({removedPosition.Quantity} shares)",
                    OrderId = Guid.NewGuid().ToString("N")[..8].ToUpper()
                };
            }
            catch (Exception ex)
            {
                return new OrderResult
                {
                    Success = false,
                    Message = $"Failed to delete position: {ex.Message}",
                    OrderId = string.Empty
                };
            }
        }

        /// <summary>
        /// Update an existing position in the system
        /// </summary>
        public async Task<OrderResult> UpdatePositionAsync(string symbol, Position position)
        {
            // TODO: Replace with call to your trading system
            // Example: return await YourTradingSystem.UpdatePositionAsync(symbol, position);
            
            await Task.Delay(50); // Simulate processing time
            
            try
            {
                if (string.IsNullOrEmpty(symbol))
                    throw new ArgumentException("Symbol is required");
                
                if (position == null)
                    throw new ArgumentException("Position data is required");
                
                if (position.Quantity == 0)
                    throw new ArgumentException("Quantity cannot be zero");
                
                if (position.AvgPrice <= 0)
                    throw new ArgumentException("Average price must be greater than zero");

                // Check if position exists
                if (!_positions.ContainsKey(symbol))
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Position for {symbol} not found. Use AddRow to create it.",
                        OrderId = string.Empty
                    };
                }

                // Preserve current price if not provided or invalid
                var existingPosition = _positions[symbol];
                if (position.CurrentPrice <= 0)
                {
                    position.CurrentPrice = existingPosition.CurrentPrice;
                }

                // Update the position
                _positions[symbol] = new Position
                {
                    Symbol = symbol,
                    Quantity = position.Quantity,
                    AvgPrice = position.AvgPrice,
                    CurrentPrice = position.CurrentPrice
                };

                return new OrderResult
                {
                    Success = true,
                    Message = $"Position updated successfully: {symbol} ({position.Quantity} shares @ ${position.AvgPrice:F2})",
                    OrderId = Guid.NewGuid().ToString("N")[..8].ToUpper()
                };
            }
            catch (Exception ex)
            {
                return new OrderResult
                {
                    Success = false,
                    Message = $"Failed to update position: {ex.Message}",
                    OrderId = string.Empty
                };
            }
        }

        /// <summary>
        /// Simulate live price movements (remove this in production)
        /// </summary>
        private async Task SimulatePriceMovements()
        {
            while (true)
            {
                await Task.Delay(2000); // Update every 2 seconds
                
                foreach (var position in _positions.Values)
                {
                    // Small random price movement
                    var change = (decimal)((_random.NextDouble() - 0.5) * 2); // -1 to +1
                    position.CurrentPrice = Math.Max(0.01m, position.CurrentPrice + change);
                    position.CurrentPrice = Math.Round(position.CurrentPrice, 2);
                }
            }
        }
    }
}

