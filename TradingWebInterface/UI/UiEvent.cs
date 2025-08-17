using System.Collections.Generic;

namespace TradingWebInterface.UI
{
    /// <summary>
    /// Generic UI event raised by the client (button click, toggle, input change).
    /// </summary>
    public class UiEvent
    {
        /// <summary>
        /// Event identifier (e.g., "PlaceOrder", "ToggleLive", "FlattenAll").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Arbitrary payload the button/input sends (e.g., { "symbol": "ES", "qty": "2" }).
        /// </summary>
        public Dictionary<string, string> Args { get; set; } = new();
    }
}