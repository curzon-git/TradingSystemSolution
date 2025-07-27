namespace TradingWebInterface.Models
{
    /// <summary>
    /// Represents a trading position
    /// </summary>
    public class Position
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PnL => (CurrentPrice - AvgPrice) * Quantity;
        public decimal PnLPercent => AvgPrice != 0 ? (PnL / (AvgPrice * Math.Abs(Quantity))) * 100 : 0;
        
        // Toggle fields
        public string Live { get; set; } = "OFF"; // "ON" or "OFF"
        public bool Flatten { get; set; } = false; // true or false
    }

    /// <summary>
    /// Account summary information
    /// </summary>
    public class AccountSummary
    {
        public decimal AccountBalance { get; set; }
        public decimal TotalPnL { get; set; }
        public Dictionary<string, Position> Positions { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Order request model
    /// </summary>
    public class OrderRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string OrderType { get; set; } = "BUY"; // BUY or SELL
    }

    /// <summary>
    /// Order result model
    /// </summary>
    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Screen field read/write models
    /// </summary>
    public class FieldReadResponse
    {
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class FieldWriteRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class FieldWriteResponse
    {
        public string FieldName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    public class AllFieldsResponse
    {
        public Dictionary<string, string> Fields { get; set; } = new();
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Response model for API operations
    /// </summary>
    public class CommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    /// <summary>
    /// Request model for adding comments to console
    /// </summary>
    public class AddCommentRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for console messages
    /// </summary>
    public class ConsoleResponse
    {
        public bool Success { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public int Count { get; set; }
    }
}

