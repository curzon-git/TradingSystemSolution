using TradingWebInterface.Models;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Interface for managing webpage data and communications
    /// </summary>
    public interface IWebPageInterfaceService
    {
        /// <summary>
        /// Get complete webpage data
        /// </summary>
        Task<WebPageData> GetWebPageDataAsync();

        /// <summary>
        /// Update complete webpage data
        /// </summary>
        Task UpdateWebPageDataAsync(WebPageData data);

        /// <summary>
        /// Get specific table data
        /// </summary>
        Task<TableData?> GetTableDataAsync(string tableId);

        /// <summary>
        /// Update specific table data
        /// </summary>
        Task UpdateTableDataAsync(string tableId, TableData tableData);

        /// <summary>
        /// Get specific field data
        /// </summary>
        Task<FieldData?> GetFieldDataAsync(string fieldId);

        /// <summary>
        /// Update specific field data
        /// </summary>
        Task UpdateFieldDataAsync(string fieldId, FieldData fieldData);

        /// <summary>
        /// Handle table operations (add, update, delete rows)
        /// </summary>
        Task<TableOperationResult> HandleTableOperationAsync(TableOperationRequest request);

        /// <summary>
        /// Process event from webpage
        /// </summary>
        Task ProcessEventAsync(EventData eventData);

        /// <summary>
        /// Get table column definitions for a specific table
        /// </summary>
        Task<List<ColumnDefinition>> GetTableColumnsAsync(string tableId);

        /// <summary>
        /// Update table column definitions
        /// </summary>
        Task UpdateTableColumnsAsync(string tableId, List<ColumnDefinition> columns);

        /// <summary>
        /// Convert positions to table data
        /// </summary>
        Task<TableData> ConvertPositionsToTableDataAsync(Dictionary<string, Position> positions);

        /// <summary>
        /// Convert table data to positions
        /// </summary>
        Task<Dictionary<string, Position>> ConvertTableDataToPositionsAsync(TableData tableData);

        /// <summary>
        /// Get enhanced positions with additional fields
        /// </summary>
        Task<List<EnhancedPosition>> GetEnhancedPositionsAsync();

        /// <summary>
        /// Update enhanced positions
        /// </summary>
        Task UpdateEnhancedPositionsAsync(List<EnhancedPosition> positions);
    }
}

