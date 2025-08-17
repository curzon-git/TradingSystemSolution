using System.Collections.Generic;

namespace TradingUi.Contracts
{
    public class UiEvent
    {
        public string Name { get; set; } = "";
        public Dictionary<string, string> Args { get; set; } = new();
    }
}
