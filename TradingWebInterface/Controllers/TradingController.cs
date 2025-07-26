using Microsoft.AspNetCore.Mvc;
using TradingWebInterface.Models;
using TradingWebInterface.Services;

namespace TradingWebInterface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TradingController : ControllerBase
    {
        private readonly ITradingSystemService _tradingSystem;
        private readonly ScreenFieldManager _screenFields;
        private readonly ILogger<TradingController> _logger;

        public TradingController(
            ITradingSystemService tradingSystem,
            ScreenFieldManager screenFields,
            ILogger<TradingController> logger)
        {
            _tradingSystem = tradingSystem;
            _screenFields = screenFields;
            _logger = logger;
        }

        #region Screen Field APIs (GetGUIField/PutGUIField equivalent)

        /// <summary>
        /// Read a field value from the screen (GetGUIField equivalent)
        /// GET /api/trading/screen/read/{fieldName}
        /// </summary>
        [HttpGet("screen/read/{fieldName}")]
        public ActionResult<FieldReadResponse> GetGUIField(string fieldName)
        {
            try
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    return BadRequest(new FieldReadResponse
                    {
                        FieldName = fieldName,
                        Success = false,
                        Error = "Field name is required"
                    });
                }

                var value = _screenFields.GetField(fieldName);
                
                _logger.LogInformation("GetGUIField: {FieldName} = '{Value}'", fieldName, value);

                return Ok(new FieldReadResponse
                {
                    FieldName = fieldName,
                    Value = value,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading field {FieldName}", fieldName);
                return StatusCode(500, new FieldReadResponse
                {
                    FieldName = fieldName,
                    Success = false,
                    Error = $"Internal error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Write a value to a screen field (PutGUIField equivalent)
        /// POST /api/trading/screen/write/{fieldName}
        /// </summary>
        [HttpPost("screen/write/{fieldName}")]
        public ActionResult<FieldWriteResponse> PutGUIField(string fieldName, [FromBody] FieldWriteRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    return BadRequest(new FieldWriteResponse
                    {
                        FieldName = fieldName,
                        Success = false,
                        Error = "Field name is required"
                    });
                }

                if (request == null)
                {
                    return BadRequest(new FieldWriteResponse
                    {
                        FieldName = fieldName,
                        Success = false,
                        Error = "Request body is required"
                    });
                }

                var success = _screenFields.SetField(fieldName, request.Value ?? string.Empty);
                
                _logger.LogInformation("PutGUIField: {FieldName} = '{Value}' (Success: {Success})", 
                    fieldName, request.Value, success);

                return Ok(new FieldWriteResponse
                {
                    FieldName = fieldName,
                    Value = request.Value ?? string.Empty,
                    Success = success
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing field {FieldName}", fieldName);
                return StatusCode(500, new FieldWriteResponse
                {
                    FieldName = fieldName,
                    Success = false,
                    Error = $"Internal error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Read all screen fields at once
        /// GET /api/trading/screen/read_all
        /// </summary>
        [HttpGet("screen/read_all")]
        public ActionResult<AllFieldsResponse> GetAllGUIFields()
        {
            try
            {
                var fields = _screenFields.GetAllFields();
                
                _logger.LogInformation("GetAllGUIFields: Retrieved {Count} fields", fields.Count);

                return Ok(new AllFieldsResponse
                {
                    Fields = fields,
                    Success = true,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading all fields");
                return StatusCode(500, new AllFieldsResponse
                {
                    Success = false
                });
            }
        }

        #endregion

        #region Button Callback APIs

        /// <summary>
        /// Place order button callback
        /// POST /api/trading/command/place_order
        /// </summary>
        [HttpPost("command/place_order")]
        public async Task<ActionResult<CommandResponse>> PlaceOrderCallback()
        {
            try
            {
                _logger.LogInformation("Place order button callback triggered");

                // Get order details from screen fields
                var (isValid, errorMessage, order) = _screenFields.GetOrderFromFields();
                
                if (!isValid)
                {
                    _screenFields.UpdateStatus($"Order validation failed: {errorMessage}");
                    _screenFields.UpdateLastCommand("PLACE_ORDER_FAILED");
                    
                    return BadRequest(new CommandResponse
                    {
                        Success = false,
                        Message = errorMessage
                    });
                }

                // Execute order through trading system
                var result = await _tradingSystem.PlaceOrderAsync(order!);
                
                if (result.Success)
                {
                    // Clear input fields on successful order
                    _screenFields.ClearFields("symbol_input", "quantity_input", "price_input");
                    _screenFields.UpdateStatus(result.Message);
                    _screenFields.UpdateLastCommand($"PLACE_ORDER_SUCCESS_{order!.Symbol}");
                    
                    _logger.LogInformation("Order placed successfully: {Message}", result.Message);
                }
                else
                {
                    _screenFields.UpdateStatus(result.Message);
                    _screenFields.UpdateLastCommand("PLACE_ORDER_FAILED");
                    
                    _logger.LogWarning("Order placement failed: {Message}", result.Message);
                }

                return Ok(new CommandResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = new { OrderId = result.OrderId }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Order placement error: {ex.Message}";
                _screenFields.UpdateStatus(errorMsg);
                _screenFields.UpdateLastCommand("PLACE_ORDER_ERROR");
                
                _logger.LogError(ex, "Error in place order callback");
                
                return StatusCode(500, new CommandResponse
                {
                    Success = false,
                    Message = errorMsg
                });
            }
        }

        /// <summary>
        /// Clear fields button callback
        /// POST /api/trading/command/clear_fields
        /// </summary>
        [HttpPost("command/clear_fields")]
        public ActionResult<CommandResponse> ClearFieldsCallback()
        {
            try
            {
                _logger.LogInformation("Clear fields button callback triggered");

                _screenFields.ClearFields("symbol_input", "quantity_input", "price_input");
                _screenFields.SetField("order_type", "BUY");
                _screenFields.UpdateStatus("All fields cleared");
                _screenFields.UpdateLastCommand("CLEAR_FIELDS");

                return Ok(new CommandResponse
                {
                    Success = true,
                    Message = "All fields cleared successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in clear fields callback");
                return StatusCode(500, new CommandResponse
                {
                    Success = false,
                    Message = $"Clear fields error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Refresh positions button callback
        /// POST /api/trading/command/refresh_positions
        /// </summary>
        [HttpPost("command/refresh_positions")]
        public async Task<ActionResult<CommandResponse>> RefreshPositionsCallback()
        {
            try
            {
                _logger.LogInformation("Refresh positions button callback triggered");

                var accountSummary = await _tradingSystem.GetAccountSummaryAsync();
                
                _screenFields.SetField("account_balance", accountSummary.AccountBalance.ToString("F2"));
                _screenFields.SetField("total_pnl", accountSummary.TotalPnL.ToString("F2"));
                _screenFields.UpdateStatus($"Positions refreshed - Total P&L: ${accountSummary.TotalPnL:F2}");
                _screenFields.UpdateLastCommand("REFRESH_POSITIONS");

                return Ok(new CommandResponse
                {
                    Success = true,
                    Message = "Positions refreshed successfully",
                    Data = new 
                    { 
                        AccountBalance = accountSummary.AccountBalance,
                        TotalPnL = accountSummary.TotalPnL,
                        PositionCount = accountSummary.Positions.Count
                    }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Refresh positions error: {ex.Message}";
                _screenFields.UpdateStatus(errorMsg);
                _screenFields.UpdateLastCommand("REFRESH_POSITIONS_ERROR");
                
                _logger.LogError(ex, "Error in refresh positions callback");
                
                return StatusCode(500, new CommandResponse
                {
                    Success = false,
                    Message = errorMsg
                });
            }
        }

        #endregion

        #region Row Management APIs

        /// <summary>
        /// Add a new position row to the webpage
        /// POST /api/trading/rows/add
        /// </summary>
        [HttpPost("rows/add")]
        public async Task<ActionResult<CommandResponse>> AddRow([FromBody] Position position)
        {
            try
            {
                _logger.LogInformation("AddRow called for symbol: {Symbol}", position?.Symbol);

                if (position == null)
                {
                    return BadRequest(new CommandResponse
                    {
                        Success = false,
                        Message = "Position data is required"
                    });
                }

                if (string.IsNullOrEmpty(position.Symbol))
                {
                    return BadRequest(new CommandResponse
                    {
                        Success = false,
                        Message = "Symbol is required"
                    });
                }

                // Add the position through the trading system
                var result = await _tradingSystem.AddPositionAsync(position);
                
                if (result.Success)
                {
                    _screenFields.UpdateStatus($"Position added: {position.Symbol}");
                    _screenFields.UpdateLastCommand($"ADD_ROW_{position.Symbol}");
                    
                    _logger.LogInformation("Position added successfully: {Symbol}", position.Symbol);
                }
                else
                {
                    _screenFields.UpdateStatus($"Failed to add position: {result.Message}");
                    _screenFields.UpdateLastCommand("ADD_ROW_FAILED");
                    
                    _logger.LogWarning("Failed to add position {Symbol}: {Message}", position.Symbol, result.Message);
                }

                return Ok(new CommandResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = new { Symbol = position.Symbol, Quantity = position.Quantity }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Add row error: {ex.Message}";
                _screenFields.UpdateStatus(errorMsg);
                _screenFields.UpdateLastCommand("ADD_ROW_ERROR");
                
                _logger.LogError(ex, "Error in AddRow for symbol {Symbol}", position?.Symbol);
                
                return StatusCode(500, new CommandResponse
                {
                    Success = false,
                    Message = errorMsg
                });
            }
        }

        /// <summary>
        /// Delete a position row from the webpage
        /// DELETE /api/trading/rows/delete/{symbol}
        /// </summary>
        [HttpDelete("rows/delete/{symbol}")]
        public async Task<ActionResult<CommandResponse>> DeleteRow(string symbol)
        {
            try
            {
                _logger.LogInformation("DeleteRow called for symbol: {Symbol}", symbol);

                if (string.IsNullOrEmpty(symbol))
                {
                    return BadRequest(new CommandResponse
                    {
                        Success = false,
                        Message = "Symbol is required"
                    });
                }

                // Delete the position through the trading system
                var result = await _tradingSystem.DeletePositionAsync(symbol);
                
                if (result.Success)
                {
                    _screenFields.UpdateStatus($"Position deleted: {symbol}");
                    _screenFields.UpdateLastCommand($"DELETE_ROW_{symbol}");
                    
                    _logger.LogInformation("Position deleted successfully: {Symbol}", symbol);
                }
                else
                {
                    _screenFields.UpdateStatus($"Failed to delete position: {result.Message}");
                    _screenFields.UpdateLastCommand("DELETE_ROW_FAILED");
                    
                    _logger.LogWarning("Failed to delete position {Symbol}: {Message}", symbol, result.Message);
                }

                return Ok(new CommandResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = new { Symbol = symbol }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Delete row error: {ex.Message}";
                _screenFields.UpdateStatus(errorMsg);
                _screenFields.UpdateLastCommand("DELETE_ROW_ERROR");
                
                _logger.LogError(ex, "Error in DeleteRow for symbol {Symbol}", symbol);
                
                return StatusCode(500, new CommandResponse
                {
                    Success = false,
                    Message = errorMsg
                });
            }
        }

        /// <summary>
        /// Update an existing position row on the webpage
        /// PUT /api/trading/rows/update/{symbol}
        /// </summary>
        [HttpPut("rows/update/{symbol}")]
        public async Task<ActionResult<CommandResponse>> UpdateRow(string symbol, [FromBody] Position position)
        {
            try
            {
                _logger.LogInformation("UpdateRow called for symbol: {Symbol}", symbol);

                if (string.IsNullOrEmpty(symbol))
                {
                    return BadRequest(new CommandResponse
                    {
                        Success = false,
                        Message = "Symbol is required"
                    });
                }

                if (position == null)
                {
                    return BadRequest(new CommandResponse
                    {
                        Success = false,
                        Message = "Position data is required"
                    });
                }

                // Ensure the position symbol matches the URL parameter
                position.Symbol = symbol;

                // Update the position through the trading system
                var result = await _tradingSystem.UpdatePositionAsync(symbol, position);
                
                if (result.Success)
                {
                    _screenFields.UpdateStatus($"Position updated: {symbol}");
                    _screenFields.UpdateLastCommand($"UPDATE_ROW_{symbol}");
                    
                    _logger.LogInformation("Position updated successfully: {Symbol}", symbol);
                }
                else
                {
                    _screenFields.UpdateStatus($"Failed to update position: {result.Message}");
                    _screenFields.UpdateLastCommand("UPDATE_ROW_FAILED");
                    
                    _logger.LogWarning("Failed to update position {Symbol}: {Message}", symbol, result.Message);
                }

                return Ok(new CommandResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = new { Symbol = symbol, Quantity = position.Quantity, AvgPrice = position.AvgPrice }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Update row error: {ex.Message}";
                _screenFields.UpdateStatus(errorMsg);
                _screenFields.UpdateLastCommand("UPDATE_ROW_ERROR");
                
                _logger.LogError(ex, "Error in UpdateRow for symbol {Symbol}", symbol);
                
                return StatusCode(500, new CommandResponse
                {
                    Success = false,
                    Message = errorMsg
                });
            }
        }

        #endregion

        #region Trading Data APIs

        /// <summary>
        /// Get current positions and account summary
        /// GET /api/trading/positions
        /// </summary>
        [HttpGet("positions")]
        public async Task<ActionResult<AccountSummary>> GetPositions()
        {
            try
            {
                var accountSummary = await _tradingSystem.GetAccountSummaryAsync();
                
                // Update screen fields with latest data
                _screenFields.SetField("account_balance", accountSummary.AccountBalance.ToString("F2"));
                _screenFields.SetField("total_pnl", accountSummary.TotalPnL.ToString("F2"));
                
                return Ok(accountSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting positions");
                return StatusCode(500, new { error = $"Error getting positions: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get system status
        /// GET /api/trading/status
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetSystemStatus()
        {
            try
            {
                var isConnected = await _tradingSystem.IsConnectedAsync();
                var statusMessage = await _tradingSystem.GetSystemStatusAsync();
                
                _screenFields.SetField("connection_status", isConnected ? "Connected" : "Disconnected");
                
                return Ok(new
                {
                    IsConnected = isConnected,
                    Status = statusMessage,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system status");
                return StatusCode(500, new { error = $"Error getting system status: {ex.Message}" });
            }
        }

        #endregion
    }
}

