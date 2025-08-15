using TradingWebInterface.Models;
using System.Collections.Concurrent;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Service for managing webpage data and communications with flexible table support
    /// </summary>
    public class WebPageInterfaceService : IWebPageInterfaceService
    {
        private readonly ITradingSystemService _tradingSystem;
        private readonly IConsoleService _console;
        private readonly ILogger<WebPageInterfaceService> _logger;
        
        // In-memory storage for webpage data (replace with database in production)
        private readonly ConcurrentDictionary<string, TableData> _tables = new();
        private readonly ConcurrentDictionary<string, FieldData> _fields = new();
        private readonly ConcurrentQueue<EventData> _events = new();
        private readonly List<EnhancedPosition> _enhancedPositions = new();
        private readonly object _lock = new object();

        public WebPageInterfaceService(
            ITradingSystemService tradingSystem,
            IConsoleService console,
            ILogger<WebPageInterfaceService> logger)
        {
            _tradingSystem = tradingSystem;
            _console = console;
            _logger = logger;
            
            // Initialize default table structures
            InitializeDefaultTables();
        }

        /// <summary>
        /// Initialize default table structures
        /// </summary>
        private void InitializeDefaultTables()
        {
            // Create enhanced positions table
            var positionsTable = new TableData
            {
                TableId = "positions_table",
                TableName = "Trading Positions",
                Columns = CreatePositionsTableColumns(),
                Settings = new TableSettings
                {
                    AllowAdd = true,
                    AllowEdit = true,
                    AllowDelete = true,
                    AllowSort = true,
                    AllowFilter = true,
                    Theme = "trading"
                }
            };

            _tables.TryAdd("positions_table", positionsTable);

            // Initialize some sample fields
            _fields.TryAdd("account_balance", new FieldData
            {
                FieldId = "account_balance",
                FieldName = "Account Balance",
                Value = 50000.00m,
                DataType = "currency",
                Format = "C2"
            });

            _fields.TryAdd("total_pl", new FieldData
            {
                FieldId = "total_pl",
                FieldName = "Total P&L",
                Value = 0.00m,
                DataType = "currency",
                Format = "C2"
            });
        }

        /// <summary>
        /// Create column definitions for positions table
        /// </summary>
        private List<ColumnDefinition> CreatePositionsTableColumns()
        {
            return new List<ColumnDefinition>
            {
                new() { Id = "symbol", Name = "Symbol", DataType = "string", Editable = false, Visible = true },
                new() { Id = "quantity", Name = "Quantity", DataType = "number", Editable = true, Visible = true },
                new() { Id = "avgPrice", Name = "Avg Price", DataType = "currency", Editable = true, Visible = true, Format = "C2" },
                new() { Id = "currentPrice", Name = "Current Price", DataType = "currency", Editable = false, Visible = true, Format = "C2" },
                new() { Id = "marketValue", Name = "Market Value", DataType = "currency", Editable = false, Visible = true, Format = "C2" },
                new() { Id = "unrealizedPL", Name = "Unrealized P&L", DataType = "currency", Editable = false, Visible = true, Format = "C2" },
                new() { Id = "unrealizedPLPercent", Name = "Unrealized P&L %", DataType = "number", Editable = false, Visible = true, Format = "P2" },
                new() { Id = "live", Name = "Live", DataType = "boolean", Editable = true, Visible = true },
                new() { Id = "flatten", Name = "Flatten", DataType = "boolean", Editable = true, Visible = true },
                new() { Id = "strategy", Name = "Strategy", DataType = "string", Editable = true, Visible = true },
                new() { Id = "account", Name = "Account", DataType = "string", Editable = true, Visible = true },
                new() { Id = "notes", Name = "Notes", DataType = "string", Editable = true, Visible = true },
                new() { Id = "stopLoss", Name = "Stop Loss", DataType = "currency", Editable = true, Visible = true, Format = "C2" },
                new() { Id = "takeProfit", Name = "Take Profit", DataType = "currency", Editable = true, Visible = true, Format = "C2" },
                new() { Id = "orderType", Name = "Order Type", DataType = "string", Editable = true, Visible = true },
                new() { Id = "lastUpdate", Name = "Last Update", DataType = "date", Editable = false, Visible = true, Format = "HH:mm:ss" }
            };
        }

        public async Task<WebPageData> GetWebPageDataAsync()
        {
            try
            {
                // Update positions table with current data
                await UpdatePositionsTableFromTradingSystem();

                return new WebPageData
                {
                    PageId = "trading_page",
                    LastUpdated = DateTime.UtcNow,
                    Tables = new Dictionary<string, TableData>(_tables),
                    Fields = new Dictionary<string, FieldData>(_fields),
                    Events = _events.ToList(),
                    CustomData = new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webpage data");
                throw;
            }
        }

        public async Task UpdateWebPageDataAsync(WebPageData data)
        {
            try
            {
                // Update tables
                foreach (var table in data.Tables)
                {
                    _tables.AddOrUpdate(table.Key, table.Value, (key, oldValue) => table.Value);
                }

                // Update fields
                foreach (var field in data.Fields)
                {
                    _fields.AddOrUpdate(field.Key, field.Value, (key, oldValue) => field.Value);
                }

                // Process events
                foreach (var eventData in data.Events)
                {
                    await ProcessEventAsync(eventData);
                }

                _logger.LogInformation("Webpage data updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating webpage data");
                throw;
            }
        }

        public async Task<TableData?> GetTableDataAsync(string tableId)
        {
            try
            {
                if (tableId == "positions_table")
                {
                    await UpdatePositionsTableFromTradingSystem();
                }

                return _tables.TryGetValue(tableId, out var tableData) ? tableData : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table data for {TableId}", tableId);
                throw;
            }
        }

        public async Task UpdateTableDataAsync(string tableId, TableData tableData)
        {
            try
            {
                _tables.AddOrUpdate(tableId, tableData, (key, oldValue) => tableData);
                tableData.LastModified = DateTime.UtcNow;

                // If this is the positions table, update the trading system
                if (tableId == "positions_table")
                {
                    await UpdateTradingSystemFromPositionsTable(tableData);
                }

                _logger.LogInformation("Table data updated for {TableId}", tableId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating table data for {TableId}", tableId);
                throw;
            }
        }

        public async Task<FieldData?> GetFieldDataAsync(string fieldId)
        {
            try
            {
                return _fields.TryGetValue(fieldId, out var fieldData) ? fieldData : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field data for {FieldId}", fieldId);
                throw;
            }
        }

        public async Task UpdateFieldDataAsync(string fieldId, FieldData fieldData)
        {
            try
            {
                _fields.AddOrUpdate(fieldId, fieldData, (key, oldValue) => fieldData);
                fieldData.LastModified = DateTime.UtcNow;

                _logger.LogInformation("Field data updated for {FieldId}", fieldId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating field data for {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<TableOperationResult> HandleTableOperationAsync(TableOperationRequest request)
        {
            try
            {
                var result = new TableOperationResult();

                if (!_tables.TryGetValue(request.TableId, out var tableData))
                {
                    result.Message = $"Table {request.TableId} not found";
                    return result;
                }

                switch (request.Operation.ToLower())
                {
                    case "add":
                        result = await HandleAddRowOperation(tableData, request);
                        break;
                    case "update":
                        result = await HandleUpdateRowOperation(tableData, request);
                        break;
                    case "delete":
                        result = await HandleDeleteRowOperation(tableData, request);
                        break;
                    case "select":
                        result = await HandleSelectRowOperation(tableData, request);
                        break;
                    default:
                        result.Message = $"Unknown operation: {request.Operation}";
                        return result;
                }

                if (result.Success)
                {
                    tableData.LastModified = DateTime.UtcNow;
                    await UpdateTableDataAsync(request.TableId, tableData);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling table operation {Operation} for {TableId}", request.Operation, request.TableId);
                return new TableOperationResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private async Task<TableOperationResult> HandleAddRowOperation(TableData tableData, TableOperationRequest request)
        {
            tableData.Rows.Add(request.Data);
            await _console.AddCommentAsync($"Row added to {tableData.TableName}");
            
            return new TableOperationResult
            {
                Success = true,
                Message = "Row added successfully",
                Data = new Dictionary<string, object> { ["rowIndex"] = tableData.Rows.Count - 1 }
            };
        }

        private async Task<TableOperationResult> HandleUpdateRowOperation(TableData tableData, TableOperationRequest request)
        {
            if (request.RowIndex.HasValue && request.RowIndex.Value < tableData.Rows.Count)
            {
                var row = tableData.Rows[request.RowIndex.Value];
                
                // Update specific column or entire row
                if (!string.IsNullOrEmpty(request.ColumnId) && request.Data.ContainsKey("value"))
                {
                    row[request.ColumnId] = request.Data["value"];
                }
                else
                {
                    // Update entire row
                    foreach (var kvp in request.Data)
                    {
                        row[kvp.Key] = kvp.Value;
                    }
                }

                await _console.AddCommentAsync($"Row {request.RowIndex} updated in {tableData.TableName}");
                
                return new TableOperationResult
                {
                    Success = true,
                    Message = "Row updated successfully",
                    Data = new Dictionary<string, object> { ["rowIndex"] = request.RowIndex.Value }
                };
            }

            return new TableOperationResult
            {
                Success = false,
                Message = "Invalid row index"
            };
        }

        private async Task<TableOperationResult> HandleDeleteRowOperation(TableData tableData, TableOperationRequest request)
        {
            if (request.RowIndex.HasValue && request.RowIndex.Value < tableData.Rows.Count)
            {
                tableData.Rows.RemoveAt(request.RowIndex.Value);
                await _console.AddCommentAsync($"Row {request.RowIndex} deleted from {tableData.TableName}");
                
                return new TableOperationResult
                {
                    Success = true,
                    Message = "Row deleted successfully"
                };
            }

            return new TableOperationResult
            {
                Success = false,
                Message = "Invalid row index"
            };
        }

        private async Task<TableOperationResult> HandleSelectRowOperation(TableData tableData, TableOperationRequest request)
        {
            await _console.AddCommentAsync($"Row {request.RowIndex} selected in {tableData.TableName}");
            
            return new TableOperationResult
            {
                Success = true,
                Message = "Row selected",
                Data = request.Data
            };
        }

        public async Task ProcessEventAsync(EventData eventData)
        {
            try
            {
                _events.Enqueue(eventData);
                
                // Keep only last 100 events
                while (_events.Count > 100)
                {
                    _events.TryDequeue(out _);
                }

                await _console.AddCommentAsync($"Event processed: {eventData.EventType} from {eventData.Source}");
                _logger.LogInformation("Event processed: {EventType} from {Source}", eventData.EventType, eventData.Source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventType}", eventData.EventType);
                throw;
            }
        }

        public async Task<List<ColumnDefinition>> GetTableColumnsAsync(string tableId)
        {
            var tableData = await GetTableDataAsync(tableId);
            return tableData?.Columns ?? new List<ColumnDefinition>();
        }

        public async Task UpdateTableColumnsAsync(string tableId, List<ColumnDefinition> columns)
        {
            if (_tables.TryGetValue(tableId, out var tableData))
            {
                tableData.Columns = columns;
                tableData.LastModified = DateTime.UtcNow;
                await UpdateTableDataAsync(tableId, tableData);
            }
        }

        public async Task<TableData> ConvertPositionsToTableDataAsync(Dictionary<string, Position> positions)
        {
            var tableData = await GetTableDataAsync("positions_table") ?? new TableData
            {
                TableId = "positions_table",
                TableName = "Trading Positions",
                Columns = CreatePositionsTableColumns()
            };

            tableData.Rows.Clear();

            foreach (var position in positions.Values)
            {
                var enhancedPosition = new EnhancedPosition
                {
                    Symbol = position.Symbol,
                    Quantity = position.Quantity,
                    AvgPrice = position.AvgPrice,
                    CurrentPrice = position.CurrentPrice,
                    Live = position.Live == "ON",
                    Flatten = position.Flatten,
                    LastUpdate = DateTime.UtcNow
                };

                tableData.Rows.Add(enhancedPosition.ToDictionary());
            }

            return tableData;
        }

        public async Task<Dictionary<string, Position>> ConvertTableDataToPositionsAsync(TableData tableData)
        {
            var positions = new Dictionary<string, Position>();

            foreach (var row in tableData.Rows)
            {
                var enhancedPosition = EnhancedPosition.FromDictionary(row);
                var position = new Position
                {
                    Symbol = enhancedPosition.Symbol,
                    Quantity = enhancedPosition.Quantity,
                    AvgPrice = enhancedPosition.AvgPrice,
                    CurrentPrice = enhancedPosition.CurrentPrice,
                    Live = enhancedPosition.Live ? "ON" : "OFF",
                    Flatten = enhancedPosition.Flatten
                };

                positions[position.Symbol] = position;
            }

            return positions;
        }

        public async Task<List<EnhancedPosition>> GetEnhancedPositionsAsync()
        {
            lock (_lock)
            {
                return new List<EnhancedPosition>(_enhancedPositions);
            }
        }

        public async Task UpdateEnhancedPositionsAsync(List<EnhancedPosition> positions)
        {
            lock (_lock)
            {
                _enhancedPositions.Clear();
                _enhancedPositions.AddRange(positions);
            }

            // Update the positions table
            var tableData = await GetTableDataAsync("positions_table");
            if (tableData != null)
            {
                tableData.Rows.Clear();
                foreach (var position in positions)
                {
                    tableData.Rows.Add(position.ToDictionary());
                }
                await UpdateTableDataAsync("positions_table", tableData);
            }
        }

        /// <summary>
        /// Update positions table from trading system data
        /// </summary>
        private async Task UpdatePositionsTableFromTradingSystem()
        {
            try
            {
                var accountSummary = await _tradingSystem.GetAccountSummaryAsync();
                var tableData = await ConvertPositionsToTableDataAsync(accountSummary.Positions);
                _tables.AddOrUpdate("positions_table", tableData, (key, oldValue) => tableData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating positions table from trading system");
            }
        }

        /// <summary>
        /// Update trading system from positions table data
        /// </summary>
        private async Task UpdateTradingSystemFromPositionsTable(TableData tableData)
        {
            try
            {
                var positions = await ConvertTableDataToPositionsAsync(tableData);
                await _tradingSystem.UpdatePositionsAsync(positions.Values.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trading system from positions table");
            }
        }
    }
}

