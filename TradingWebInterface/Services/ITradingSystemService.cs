using TradingWebInterface.Models;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Interface to your existing C# trading system
    /// Implement this interface to connect to your actual trading system
    /// </summary>
    public interface ITradingSystemService
    {
        /// <summary>
        /// Get current account summary with positions and P&L
        /// </summary>
        Task<AccountSummary> GetAccountSummaryAsync();

        /// <summary>
        /// Get current positions
        /// </summary>
        Task<Dictionary<string, Position>> GetPositionsAsync();

        /// <summary>
        /// Place a trading order
        /// </summary>
        Task<OrderResult> PlaceOrderAsync(OrderRequest order);

        /// <summary>
        /// Get current market price for a symbol
        /// </summary>
        Task<decimal> GetCurrentPriceAsync(string symbol);

        /// <summary>
        /// Check if the trading system is connected and ready
        /// </summary>
        Task<bool> IsConnectedAsync();

        /// <summary>
        /// Get system status message
        /// </summary>
        Task<string> GetSystemStatusAsync();

        /// <summary>
        /// Add a new position (row) to the system
        /// </summary>
        Task<OrderResult> AddPositionAsync(Position position);

        /// <summary>
        /// Delete a position (row) from the system
        /// </summary>
        Task<OrderResult> DeletePositionAsync(string symbol);

        /// <summary>
        /// Update an existing position (row) in the system
        /// </summary>
        Task<OrderResult> UpdatePositionAsync(string symbol, Position position);

        /// <summary>
        /// Toggle the Live field for a position
        /// </summary>
        Task<OrderResult> ToggleLiveAsync(string symbol);

        /// <summary>
        /// Toggle the Flatten field for a position
        /// </summary>
        Task<OrderResult> ToggleFlattenAsync(string symbol);

        /// <summary>
        /// Update all positions with new data (for external refresh)
        /// </summary>
        Task UpdatePositionsAsync(Position[] positions);
    }
}

