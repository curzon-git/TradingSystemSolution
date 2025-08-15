using System;
using System.Threading.Tasks;
using TradingWebInterface.UI;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Abstraction for bi-directional UI exchange.
    /// - Publish UiState to all clients.
    /// - Get/Set small fields.
    /// - Subscribe to UI events (buttons, inputs, toggles).
    /// </summary>
    public interface IUiBus
    {
        /// <summary>
        /// Publish a complete (or partial) UI state to all connected clients.
        /// </summary>
        Task PublishStateAsync(UiState state);

        /// <summary>
        /// Get/Set small key-value fields.
        /// </summary>
        string GetField(string key);
        void SetField(string key, string value);

        /// <summary>
        /// Subscribe to UI events. Handlers are invoked sequentially (awaited).
        /// </summary>
        event Func<UiEvent, Task> OnEvent;

        /// <summary>
        /// Emit an event programmatically (or from a hub).
        /// </summary>
        Task EmitAsync(UiEvent evt);
    }
}