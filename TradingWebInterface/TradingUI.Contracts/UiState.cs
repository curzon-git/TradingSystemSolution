using System.Collections.Generic;

namespace TradingUi.Contracts
{
    public class UiState
    {
        public Dictionary<string, string> Fields { get; set; } = new();
        public Dictionary<string, List<Dictionary<string, object>>> Tables { get; set; } = new();
        public Dictionary<string, object> Meta { get; set; } = new();
    }
}
