using System.Collections.Concurrent;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Manages screen fields for GetGUIField/PutGUIField functionality
    /// Thread-safe implementation for multi-user access
    /// </summary>
    public class ScreenFieldManager
    {
        private readonly ConcurrentDictionary<string, string> _screenFields;

        public ScreenFieldManager()
        {
            _screenFields = new ConcurrentDictionary<string, string>();
            
            // Initialize default field values
            InitializeDefaultFields();
        }

        /// <summary>
        /// Get field value (equivalent to GetGUIField)
        /// </summary>
        public string GetField(string fieldName)
        {
            return _screenFields.GetValueOrDefault(fieldName, string.Empty);
        }

        /// <summary>
        /// Set field value (equivalent to PutGUIField)
        /// </summary>
        public bool SetField(string fieldName, string value)
        {
            try
            {
                _screenFields.AddOrUpdate(fieldName, value, (key, oldValue) => value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all field values
        /// </summary>
        public Dictionary<string, string> GetAllFields()
        {
            return new Dictionary<string, string>(_screenFields);
        }

        /// <summary>
        /// Clear specific field
        /// </summary>
        public bool ClearField(string fieldName)
        {
            return SetField(fieldName, string.Empty);
        }

        /// <summary>
        /// Clear multiple fields
        /// </summary>
        public void ClearFields(params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                ClearField(fieldName);
            }
        }

        /// <summary>
        /// Check if field exists
        /// </summary>
        public bool FieldExists(string fieldName)
        {
            return _screenFields.ContainsKey(fieldName);
        }

        /// <summary>
        /// Update status display field
        /// </summary>
        public void UpdateStatus(string message)
        {
            SetField("status_display", message);
            SetField("last_updated", DateTime.Now.ToString("HH:mm:ss"));
        }

        /// <summary>
        /// Update last command field
        /// </summary>
        public void UpdateLastCommand(string command)
        {
            SetField("last_command", command);
        }

        /// <summary>
        /// Initialize default field values
        /// </summary>
        private void InitializeDefaultFields()
        {
            var defaultFields = new Dictionary<string, string>
            {
                ["symbol_input"] = "",
                ["quantity_input"] = "",
                ["price_input"] = "",
                ["order_type"] = "BUY",
                ["status_display"] = "System Ready",
                ["last_command"] = "None",
                ["last_updated"] = DateTime.Now.ToString("HH:mm:ss"),
                ["account_balance"] = "0.00",
                ["total_pnl"] = "0.00",
                ["connection_status"] = "Connected"
            };

            foreach (var field in defaultFields)
            {
                _screenFields.TryAdd(field.Key, field.Value);
            }
        }

        /// <summary>
        /// Get order request from current field values
        /// </summary>
        public (bool IsValid, string ErrorMessage, Models.OrderRequest? Order) GetOrderFromFields()
        {
            try
            {
                var symbol = GetField("symbol_input").Trim().ToUpper();
                var quantityStr = GetField("quantity_input").Trim();
                var priceStr = GetField("price_input").Trim();
                var orderType = GetField("order_type").Trim().ToUpper();

                if (string.IsNullOrEmpty(symbol))
                    return (false, "Symbol is required", null);

                if (!int.TryParse(quantityStr, out var quantity) || quantity <= 0)
                    return (false, "Valid quantity is required", null);

                if (!decimal.TryParse(priceStr, out var price) || price <= 0)
                    return (false, "Valid price is required", null);

                if (orderType != "BUY" && orderType != "SELL")
                    return (false, "Order type must be BUY or SELL", null);

                var order = new Models.OrderRequest
                {
                    Symbol = symbol,
                    Quantity = quantity,
                    Price = price,
                    OrderType = orderType
                };

                return (true, string.Empty, order);
            }
            catch (Exception ex)
            {
                return (false, $"Error parsing order: {ex.Message}", null);
            }
        }
    }
}

