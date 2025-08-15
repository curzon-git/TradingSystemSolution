using System.Collections.Generic;

namespace TradingWebInterface.UI
{
    /// <summary>
    /// Generic, serializable UI state. Your main program should only need to populate
    /// this structure and push it to the UI; the web layer renders it and routes events back.
    /// </summary>
    public class UiState
    {
        /// <summary>
        /// Free-form key-value "fields" (status text, small values).
        /// </summary>
        public Dictionary<string, string> Fields { get; set; } = new();

        /// <summary>
        /// Named tables for grid-like displays (e.g., Positions, Orders).
        /// Each table is a list of rows, each row is a dictionary of column -> value.
        /// </summary>
        public Dictionary<string, List<Dictionary<string, object>>> Tables { get; set; } = new();

        /// <summary>
        /// Optional metadata to help the front-end style or arrange content.
        /// </summary>
        public Dictionary<string, object> Meta { get; set; } = new();
    }
}