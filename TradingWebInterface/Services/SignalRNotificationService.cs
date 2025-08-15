using Microsoft.AspNetCore.SignalR;
using TradingWebInterface.Hubs;
using TradingWebInterface.Models;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Service to handle SignalR notifications for real-time updates
    /// </summary>
    public interface ISignalRNotificationService
    {
        Task NotifyPositionsUpdated(AccountSummary accountSummary);
        Task NotifyConsoleUpdated();
        Task NotifyOrderPlaced(OrderResult result);
        Task NotifyPositionToggled(string symbol, string field, string newValue);
    }

    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<TradingHub> _hubContext;
        private readonly IConsoleService _console;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(
            IHubContext<TradingHub> hubContext,
            IConsoleService console,
            ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _console = console;
            _logger = logger;
        }

        /// <summary>
        /// Notify all clients that positions have been updated
        /// </summary>
        public async Task NotifyPositionsUpdated(AccountSummary accountSummary)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("PositionsUpdate", accountSummary);
                _logger.LogInformation("Positions update sent to all clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying positions updated");
            }
        }

        /// <summary>
        /// Notify all clients that console has been updated
        /// </summary>
        public async Task NotifyConsoleUpdated()
        {
            try
            {
                var messages = await _console.GetMessagesAsync();
                await _hubContext.Clients.All.SendAsync("ConsoleUpdate", messages);
                _logger.LogInformation("Console update sent to all clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying console updated");
            }
        }

        /// <summary>
        /// Notify all clients that an order has been placed
        /// </summary>
        public async Task NotifyOrderPlaced(OrderResult result)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("OrderPlaced", result);
                _logger.LogInformation($"Order placed notification sent: {result.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying order placed");
            }
        }

        /// <summary>
        /// Notify all clients that a position field has been toggled
        /// </summary>
        public async Task NotifyPositionToggled(string symbol, string field, string newValue)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("PositionToggled", new { symbol, field, newValue });
                _logger.LogInformation($"Position toggle notification sent: {symbol} {field} = {newValue}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying position toggled");
            }
        }
    }
}

