using System.ComponentModel.DataAnnotations;

namespace TradingWebInterface.Models
{
    /// <summary>
    /// Master container for all webpage data and communications
    /// </summary>
    public class WebPageData
    {
        public string PageId { get; set; } = "";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Dictionary<string, TableData> Tables { get; set; } = new();
        public Dictionary<string, FieldData> Fields { get; set; } = new();
        public List<EventData> Events { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    /// <summary>
    /// Flexible table structure that can handle dynamic columns and data
    /// </summary>
    public class TableData
    {
        public string TableId { get; set; } = "";
        public string TableName { get; set; } = "";
        public List<ColumnDefinition> Columns { get; set; } = new();
        public List<Dictionary<string, object>> Rows { get; set; } = new();
        public TableSettings Settings { get; set; } = new();
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Column definition for dynamic table structure
    /// </summary>
    public class ColumnDefinition
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DataType { get; set; } = "string"; // string, number, boolean, date, currency
        public bool Editable { get; set; } = false;
        public bool Visible { get; set; } = true;
        public string Format { get; set; } = ""; // For display formatting
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Table settings and permissions
    /// </summary>
    public class TableSettings
    {
        public bool AllowAdd { get; set; } = true;
        public bool AllowEdit { get; set; } = true;
        public bool AllowDelete { get; set; } = true;
        public bool AllowSort { get; set; } = true;
        public bool AllowFilter { get; set; } = true;
        public string Theme { get; set; } = "default";
        public Dictionary<string, object> CustomSettings { get; set; } = new();
    }

    /// <summary>
    /// Individual field data for form fields and other UI elements
    /// </summary>
    public class FieldData
    {
        public string FieldId { get; set; } = "";
        public string FieldName { get; set; } = "";
        public object Value { get; set; } = "";
        public string DataType { get; set; } = "string";
        public bool ReadOnly { get; set; } = false;
        public string Format { get; set; } = "";
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Event data for notifications between webpage and C# program
    /// </summary>
    public class EventData
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = "";
        public string Source { get; set; } = ""; // Table, Field, Button, etc.
        public string SourceId { get; set; } = "";
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = "";
    }

    /// <summary>
    /// Standard event types for consistent communication
    /// </summary>
    public static class EventTypes
    {
        // Table Events
        public const string TABLE_ROW_ADDED = "table.row.added";
        public const string TABLE_ROW_UPDATED = "table.row.updated";
        public const string TABLE_ROW_DELETED = "table.row.deleted";
        public const string TABLE_ROW_SELECTED = "table.row.selected";
        public const string TABLE_CELL_CHANGED = "table.cell.changed";
        public const string TABLE_SORTED = "table.sorted";
        public const string TABLE_FILTERED = "table.filtered";
        
        // Field Events
        public const string FIELD_CHANGED = "field.changed";
        public const string FIELD_FOCUSED = "field.focused";
        public const string FIELD_BLURRED = "field.blurred";
        
        // Button Events
        public const string BUTTON_CLICKED = "button.clicked";
        public const string TOGGLE_CHANGED = "toggle.changed";
        
        // Page Events
        public const string PAGE_LOADED = "page.loaded";
        public const string PAGE_UNLOADED = "page.unloaded";
        public const string PAGE_RESIZED = "page.resized";
        
        // Custom Events
        public const string CUSTOM_ACTION = "custom.action";
    }

    /// <summary>
    /// Enhanced position model with additional fields for flexible table structure
    /// </summary>
    public class EnhancedPosition
    {
        public string Symbol { get; set; } = "";
        public int Quantity { get; set; } = 0;
        public decimal AvgPrice { get; set; } = 0;
        public decimal CurrentPrice { get; set; } = 0;
        public decimal MarketValue => Quantity * CurrentPrice;
        public decimal UnrealizedPL => (CurrentPrice - AvgPrice) * Quantity;
        public decimal UnrealizedPLPercent => AvgPrice != 0 ? (CurrentPrice - AvgPrice) / AvgPrice : 0;
        public bool Live { get; set; } = false;
        public bool Flatten { get; set; } = false;
        public string Strategy { get; set; } = "";
        public string Account { get; set; } = "";
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = "";
        public decimal StopLoss { get; set; } = 0;
        public decimal TakeProfit { get; set; } = 0;
        public string OrderType { get; set; } = "";
        public Dictionary<string, object> CustomFields { get; set; } = new();

        /// <summary>
        /// Convert to dictionary for flexible table display
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>
            {
                ["symbol"] = Symbol,
                ["quantity"] = Quantity,
                ["avgPrice"] = AvgPrice,
                ["currentPrice"] = CurrentPrice,
                ["marketValue"] = MarketValue,
                ["unrealizedPL"] = UnrealizedPL,
                ["unrealizedPLPercent"] = UnrealizedPLPercent,
                ["live"] = Live,
                ["flatten"] = Flatten,
                ["strategy"] = Strategy,
                ["account"] = Account,
                ["lastUpdate"] = LastUpdate,
                ["notes"] = Notes,
                ["stopLoss"] = StopLoss,
                ["takeProfit"] = TakeProfit,
                ["orderType"] = OrderType
            };

            // Add custom fields
            foreach (var customField in CustomFields)
            {
                dict[customField.Key] = customField.Value;
            }

            return dict;
        }

        /// <summary>
        /// Create from dictionary for flexible data input
        /// </summary>
        public static EnhancedPosition FromDictionary(Dictionary<string, object> dict)
        {
            var position = new EnhancedPosition();

            if (dict.TryGetValue("symbol", out var symbol)) position.Symbol = symbol?.ToString() ?? "";
            if (dict.TryGetValue("quantity", out var quantity)) position.Quantity = Convert.ToInt32(quantity);
            if (dict.TryGetValue("avgPrice", out var avgPrice)) position.AvgPrice = Convert.ToDecimal(avgPrice);
            if (dict.TryGetValue("currentPrice", out var currentPrice)) position.CurrentPrice = Convert.ToDecimal(currentPrice);
            if (dict.TryGetValue("live", out var live)) position.Live = Convert.ToBoolean(live);
            if (dict.TryGetValue("flatten", out var flatten)) position.Flatten = Convert.ToBoolean(flatten);
            if (dict.TryGetValue("strategy", out var strategy)) position.Strategy = strategy?.ToString() ?? "";
            if (dict.TryGetValue("account", out var account)) position.Account = account?.ToString() ?? "";
            if (dict.TryGetValue("notes", out var notes)) position.Notes = notes?.ToString() ?? "";
            if (dict.TryGetValue("stopLoss", out var stopLoss)) position.StopLoss = Convert.ToDecimal(stopLoss);
            if (dict.TryGetValue("takeProfit", out var takeProfit)) position.TakeProfit = Convert.ToDecimal(takeProfit);
            if (dict.TryGetValue("orderType", out var orderType)) position.OrderType = orderType?.ToString() ?? "";

            // Add any additional fields as custom fields
            var standardFields = new HashSet<string> 
            { 
                "symbol", "quantity", "avgPrice", "currentPrice", "marketValue", 
                "unrealizedPL", "unrealizedPLPercent", "live", "flatten", "strategy", 
                "account", "lastUpdate", "notes", "stopLoss", "takeProfit", "orderType" 
            };

            foreach (var kvp in dict)
            {
                if (!standardFields.Contains(kvp.Key))
                {
                    position.CustomFields[kvp.Key] = kvp.Value;
                }
            }

            return position;
        }
    }

    /// <summary>
    /// Table operation request for handling table modifications
    /// </summary>
    public class TableOperationRequest
    {
        public string TableId { get; set; } = "";
        public string Operation { get; set; } = ""; // add, update, delete, select
        public int? RowIndex { get; set; }
        public string? ColumnId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Table operation result
    /// </summary>
    public class TableOperationResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = "";
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}

