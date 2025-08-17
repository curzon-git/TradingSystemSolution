using Microsoft.AspNetCore.SignalR;
using TradingWebInterface.Models;
using TradingWebInterface.Services;

namespace TradingWebInterface.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time trading communication with enhanced webpage interface
    /// </summary>
    public class TradingHub : Hub
    {
        private readonly ITradingSystemService _tradingSystem;
        private readonly IConsoleService _console;
        private readonly IWebPageInterfaceService _webPageInterface;
        private readonly ILogger<TradingHub> _logger;

        public TradingHub(
            ITradingSystemService tradingSystem, 
            IConsoleService console,
            IWebPageInterfaceService webPageInterface,
            ILogger<TradingHub> logger)
        {
            _tradingSystem = tradingSystem;
            _console = console;
            _webPageInterface = webPageInterface;
            _logger = logger;
        }

        /// <summary>
        /// Client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await _console.AddCommentAsync($"Client connected: {Context.ConnectionId}");
            
            // Send initial data to the newly connected client
            await SendPositionsUpdate();
            await SendConsoleUpdate();
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await _console.AddCommentAsync($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Get current positions (called by client)
        /// </summary>
        public async Task GetPositions()
        {
            try
            {
                await SendPositionsUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions");
                await Clients.Caller.SendAsync("Error", $"Failed to get positions: {ex.Message}");
            }
        }

        /// <summary>
        /// Place an order (called by client)
        /// </summary>
        public async Task PlaceOrder(OrderRequest order)
        {
            try
            {
                _logger.LogInformation($"Placing order via SignalR: {order.Symbol} {order.Quantity} @ {order.Price}");
                
                var result = await _tradingSystem.PlaceOrderAsync(order);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("OrderResult", result);
                
                // Update all clients with new positions
                await SendPositionsUpdate();
                
                // Log to console
                await _console.AddCommentAsync($"Order placed: {order.Symbol} {order.Quantity} @ {order.Price} - {result.Message}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                await Clients.Caller.SendAsync("Error", $"Failed to place order: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle Live field (called by client)
        /// </summary>
        public async Task ToggleLive(string symbol)
        {
            try
            {
                _logger.LogInformation($"Toggling Live for {symbol} via SignalR");
                
                var result = await _tradingSystem.ToggleLiveAsync(symbol);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("ToggleResult", result);
                
                // Update all clients with new positions
                await SendPositionsUpdate();
                
                // Log to console
                await _console.AddCommentAsync($"Live toggled for {symbol}: {result.Message}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling Live");
                await Clients.Caller.SendAsync("Error", $"Failed to toggle Live: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle Flatten field (called by client)
        /// </summary>
        public async Task ToggleFlatten(string symbol)
        {
            try
            {
                _logger.LogInformation($"Toggling Flatten for {symbol} via SignalR");
                
                var result = await _tradingSystem.ToggleFlattenAsync(symbol);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("ToggleResult", result);
                
                // Update all clients with new positions
                await SendPositionsUpdate();
                
                // Log to console
                await _console.AddCommentAsync($"Flatten toggled for {symbol}: {result.Message}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling Flatten");
                await Clients.Caller.SendAsync("Error", $"Failed to toggle Flatten: {ex.Message}");
            }
        }

        /// <summary>
        /// Add position (called by client)
        /// </summary>
        public async Task AddPosition(Position position)
        {
            try
            {
                _logger.LogInformation($"Adding position via SignalR: {position.Symbol}");
                
                var result = await _tradingSystem.AddPositionAsync(position);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("AddPositionResult", result);
                
                // Update all clients with new positions
                await SendPositionsUpdate();
                
                // Log to console
                await _console.AddCommentAsync($"Position added: {position.Symbol} - {result.Message}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding position");
                await Clients.Caller.SendAsync("Error", $"Failed to add position: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete position (called by client)
        /// </summary>
        public async Task DeletePosition(string symbol)
        {
            try
            {
                _logger.LogInformation($"Deleting position via SignalR: {symbol}");
                
                var result = await _tradingSystem.DeletePositionAsync(symbol);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("DeletePositionResult", result);
                
                // Update all clients with new positions
                await SendPositionsUpdate();
                
                // Log to console
                await _console.AddCommentAsync($"Position deleted: {symbol} - {result.Message}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting position");
                await Clients.Caller.SendAsync("Error", $"Failed to delete position: {ex.Message}");
            }
        }

        /// <summary>
        /// Update position (called by client)
        /// </summary>
        public async Task UpdatePosition(string symbol, Position position)
        {
            try
            {
                _logger.LogInformation($"Updating position via SignalR: {symbol}");
                
                var result = await _tradingSystem.UpdatePositionAsync(symbol, position);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("UpdatePositionResult", result);
                
                // Update all clients with new positions
                await SendPositionsUpdate();
                
                // Log to console
                await _console.AddCommentAsync($"Position updated: {symbol} - {result.Message}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating position");
                await Clients.Caller.SendAsync("Error", $"Failed to update position: {ex.Message}");
            }
        }

        /// <summary>
        /// Add console comment (called by client)
        /// </summary>
        public async Task AddConsoleComment(string message)
        {
            try
            {
                await _console.AddCommentAsync(message);
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding console comment");
                await Clients.Caller.SendAsync("Error", $"Failed to add console comment: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear console (called by client)
        /// </summary>
        public async Task ClearConsole()
        {
            try
            {
                await _console.ClearAsync();
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing console");
                await Clients.Caller.SendAsync("Error", $"Failed to clear console: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh webpage with new positions (called externally)
        /// </summary>
        public async Task RefreshWebPage(Position[] positions)
        {
            try
            {
                _logger.LogInformation($"RefreshWebPage called via SignalR with {positions.Length} positions");
                
                await _tradingSystem.UpdatePositionsAsync(positions);
                
                // Update all connected clients
                await SendPositionsUpdate();
                
                await _console.AddCommentAsync($"Webpage refreshed with {positions.Length} positions");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing webpage");
                await Clients.All.SendAsync("Error", $"Failed to refresh webpage: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current webpage positions (called externally)
        /// </summary>
        public async Task<Position[]> GetWebPage()
        {
            try
            {
                _logger.LogInformation("GetWebPage called via SignalR");
                
                var accountSummary = await _tradingSystem.GetAccountSummaryAsync();
                var positions = accountSummary.Positions.Values.ToArray();
                
                _logger.LogInformation($"Retrieved {positions.Length} positions from webpage");
                
                return positions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webpage positions");
                return Array.Empty<Position>();
            }
        }

        // ========== ENHANCED WEBPAGE INTERFACE METHODS ==========

        /// <summary>
        /// Get complete webpage data (called by client)
        /// </summary>
        public async Task<WebPageData> GetWebPageData()
        {
            try
            {
                _logger.LogInformation("Getting complete webpage data via SignalR");
                var webPageData = await _webPageInterface.GetWebPageDataAsync();
                return webPageData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting webpage data");
                await Clients.Caller.SendAsync("Error", $"Failed to get webpage data: {ex.Message}");
                return new WebPageData();
            }
        }

        /// <summary>
        /// Receive complete webpage data from client
        /// </summary>
        public async Task ReceiveWebPageData(WebPageData data)
        {
            try
            {
                _logger.LogInformation("Receiving webpage data from client via SignalR");
                await _webPageInterface.UpdateWebPageDataAsync(data);
                
                // Notify all clients of the update
                await Clients.All.SendAsync("WebPageDataUpdate", data);
                
                await _console.AddCommentAsync("Webpage data updated from client");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving webpage data");
                await Clients.Caller.SendAsync("Error", $"Failed to update webpage data: {ex.Message}");
            }
        }

        /// <summary>
        /// Get specific table data (called by client)
        /// </summary>
        public async Task<TableData?> GetTableData(string tableId)
        {
            try
            {
                _logger.LogInformation("Getting table data for {TableId} via SignalR", tableId);
                return await _webPageInterface.GetTableDataAsync(tableId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table data for {TableId}", tableId);
                await Clients.Caller.SendAsync("Error", $"Failed to get table data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Receive table data update from client
        /// </summary>
        public async Task ReceiveTableUpdate(string tableId, TableData tableData)
        {
            try
            {
                _logger.LogInformation("Receiving table update for {TableId} from client via SignalR", tableId);
                await _webPageInterface.UpdateTableDataAsync(tableId, tableData);
                
                // Notify all clients of the table update
                await Clients.All.SendAsync("TableDataUpdate", tableId, tableData);
                
                await _console.AddCommentAsync($"Table {tableId} updated from client");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving table update for {TableId}", tableId);
                await Clients.Caller.SendAsync("Error", $"Failed to update table: {ex.Message}");
            }
        }

        /// <summary>
        /// Get specific field data (called by client)
        /// </summary>
        public async Task<FieldData?> GetFieldData(string fieldId)
        {
            try
            {
                _logger.LogInformation("Getting field data for {FieldId} via SignalR", fieldId);
                return await _webPageInterface.GetFieldDataAsync(fieldId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting field data for {FieldId}", fieldId);
                await Clients.Caller.SendAsync("Error", $"Failed to get field data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Receive field data update from client
        /// </summary>
        public async Task ReceiveFieldUpdate(string fieldId, FieldData fieldData)
        {
            try
            {
                _logger.LogInformation("Receiving field update for {FieldId} from client via SignalR", fieldId);
                await _webPageInterface.UpdateFieldDataAsync(fieldId, fieldData);
                
                // Notify all clients of the field update
                await Clients.All.SendAsync("FieldDataUpdate", fieldId, fieldData);
                
                await _console.AddCommentAsync($"Field {fieldId} updated from client");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving field update for {FieldId}", fieldId);
                await Clients.Caller.SendAsync("Error", $"Failed to update field: {ex.Message}");
            }
        }

        /// <summary>
        /// Receive event notification from client
        /// </summary>
        public async Task ReceiveEvent(EventData eventData)
        {
            try
            {
                _logger.LogInformation("Receiving event {EventType} from client via SignalR", eventData.EventType);
                await _webPageInterface.ProcessEventAsync(eventData);
                
                // Notify all clients of the event
                await Clients.All.SendAsync("EventNotification", eventData);
                
                await _console.AddCommentAsync($"Event received: {eventData.EventType} from {eventData.Source}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving event {EventType}", eventData.EventType);
                await Clients.Caller.SendAsync("Error", $"Failed to process event: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle table operations (add, update, delete rows)
        /// </summary>
        public async Task HandleTableOperation(TableOperationRequest request)
        {
            try
            {
                _logger.LogInformation("Handling table operation {Operation} for {TableId} via SignalR", request.Operation, request.TableId);
                var result = await _webPageInterface.HandleTableOperationAsync(request);
                
                // Send result back to caller
                await Clients.Caller.SendAsync("TableOperationResult", result);
                
                if (result.Success)
                {
                    // Notify all clients of the table change
                    var updatedTableData = await _webPageInterface.GetTableDataAsync(request.TableId);
                    if (updatedTableData != null)
                    {
                        await Clients.All.SendAsync("TableDataUpdate", request.TableId, updatedTableData);
                    }
                }
                
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling table operation {Operation} for {TableId}", request.Operation, request.TableId);
                await Clients.Caller.SendAsync("Error", $"Failed to handle table operation: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle table row addition
        /// </summary>
        public async Task HandleTableRowAdd(string tableId, Dictionary<string, object> rowData)
        {
            var request = new TableOperationRequest
            {
                TableId = tableId,
                Operation = "add",
                Data = rowData
            };
            
            await HandleTableOperation(request);
        }

        /// <summary>
        /// Handle table row update
        /// </summary>
        public async Task HandleTableRowUpdate(string tableId, int rowIndex, Dictionary<string, object> rowData)
        {
            var request = new TableOperationRequest
            {
                TableId = tableId,
                Operation = "update",
                RowIndex = rowIndex,
                Data = rowData
            };
            
            await HandleTableOperation(request);
        }

        /// <summary>
        /// Handle table row deletion
        /// </summary>
        public async Task HandleTableRowDelete(string tableId, int rowIndex)
        {
            var request = new TableOperationRequest
            {
                TableId = tableId,
                Operation = "delete",
                RowIndex = rowIndex
            };
            
            await HandleTableOperation(request);
        }

        /// <summary>
        /// Handle table cell change
        /// </summary>
        public async Task HandleTableCellChange(string tableId, int rowIndex, string columnId, object newValue)
        {
            var request = new TableOperationRequest
            {
                TableId = tableId,
                Operation = "update",
                RowIndex = rowIndex,
                ColumnId = columnId,
                Data = new Dictionary<string, object> { ["value"] = newValue }
            };
            
            await HandleTableOperation(request);
        }

        /// <summary>
        /// Get table column definitions
        /// </summary>
        public async Task<List<ColumnDefinition>> GetTableColumns(string tableId)
        {
            try
            {
                _logger.LogInformation("Getting table columns for {TableId} via SignalR", tableId);
                return await _webPageInterface.GetTableColumnsAsync(tableId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table columns for {TableId}", tableId);
                await Clients.Caller.SendAsync("Error", $"Failed to get table columns: {ex.Message}");
                return new List<ColumnDefinition>();
            }
        }

        /// <summary>
        /// Update table column definitions
        /// </summary>
        public async Task UpdateTableColumns(string tableId, List<ColumnDefinition> columns)
        {
            try
            {
                _logger.LogInformation("Updating table columns for {TableId} via SignalR", tableId);
                await _webPageInterface.UpdateTableColumnsAsync(tableId, columns);
                
                // Notify all clients of the column update
                await Clients.All.SendAsync("TableColumnsUpdate", tableId, columns);
                
                await _console.AddCommentAsync($"Table columns updated for {tableId}");
                await SendConsoleUpdate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating table columns for {TableId}", tableId);
                await Clients.Caller.SendAsync("Error", $"Failed to update table columns: {ex.Message}");
            }
        }

        // ========== PRIVATE HELPER METHODS ==========

        /// <summary>
        /// Send positions update to all clients
        /// </summary>
        private async Task SendPositionsUpdate()
        {
            try
            {
                var accountSummary = await _tradingSystem.GetAccountSummaryAsync();
                await Clients.All.SendAsync("PositionsUpdate", accountSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending positions update");
            }
        }

        /// <summary>
        /// Send console update to all clients
        /// </summary>
        private async Task SendConsoleUpdate()
        {
            try
            {
                var messages = await _console.GetMessagesAsync();
                await Clients.All.SendAsync("ConsoleUpdate", messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending console update");
            }
        }
    }
}

