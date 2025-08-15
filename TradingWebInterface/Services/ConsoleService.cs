using System.Collections.Concurrent;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// Service for managing console-style messages that can be displayed in a text box
    /// </summary>
    public interface IConsoleService
    {
        /// <summary>
        /// Add a comment/message to the console
        /// </summary>
        void AddComment(string message);
        
        /// <summary>
        /// Add a comment/message to the console (async version)
        /// </summary>
        Task AddCommentAsync(string message);
        
        /// <summary>
        /// Get all console messages
        /// </summary>
        List<string> GetAllMessages();
        
        /// <summary>
        /// Get all console messages (async version)
        /// </summary>
        Task<List<string>> GetMessagesAsync();
        
        /// <summary>
        /// Clear all console messages
        /// </summary>
        void ClearMessages();
        
        /// <summary>
        /// Clear all console messages (async version)
        /// </summary>
        Task ClearAsync();
        
        /// <summary>
        /// Get the latest messages (for real-time updates)
        /// </summary>
        List<string> GetRecentMessages(int count = 50);
    }

    /// <summary>
    /// Implementation of console service for managing console messages
    /// </summary>
    public class ConsoleService : IConsoleService
    {
        private readonly ConcurrentQueue<string> _messages;
        private readonly object _lock = new object();
        private const int MaxMessages = 1000; // Keep last 1000 messages

        public ConsoleService()
        {
            _messages = new ConcurrentQueue<string>();
            
            // Add initial welcome message
            AddComment("=== Trading System Console Started ===");
            AddComment($"Console initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            AddComment("Ready for trading operations...");
        }

        /// <summary>
        /// Add a comment/message to the console with timestamp
        /// </summary>
        public void AddComment(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            lock (_lock)
            {
                var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
                _messages.Enqueue(timestampedMessage);

                // Keep only the last MaxMessages
                while (_messages.Count > MaxMessages)
                {
                    _messages.TryDequeue(out _);
                }
            }
        }

        /// <summary>
        /// Get all console messages
        /// </summary>
        public List<string> GetAllMessages()
        {
            lock (_lock)
            {
                return _messages.ToList();
            }
        }

        /// <summary>
        /// Clear all console messages
        /// </summary>
        public void ClearMessages()
        {
            lock (_lock)
            {
                _messages.Clear();
                AddComment("Console cleared");
            }
        }

        /// <summary>
        /// Get the latest messages (for real-time updates)
        /// </summary>
        public List<string> GetRecentMessages(int count = 50)
        {
            lock (_lock)
            {
                var allMessages = _messages.ToList();
                return allMessages.TakeLast(count).ToList();
            }
        }

        /// <summary>
        /// Add a comment/message to the console with timestamp (async version)
        /// </summary>
        public Task AddCommentAsync(string message)
        {
            AddComment(message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get all console messages (async version)
        /// </summary>
        public Task<List<string>> GetMessagesAsync()
        {
            return Task.FromResult(GetAllMessages());
        }

        /// <summary>
        /// Clear all console messages (async version)
        /// </summary>
        public Task ClearAsync()
        {
            ClearMessages();
            return Task.CompletedTask;
        }
    }
}

