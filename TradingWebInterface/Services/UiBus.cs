using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TradingWebInterface.Hubs;
using TradingWebInterface.UI;

namespace TradingWebInterface.Services
{
    /// <summary>
    /// SignalR-backed UiBus implementation.
    /// Uses ScreenFieldManager for field storage; broadcasts UiState via TradingHub.
    /// </summary>
    public class UiBus : IUiBus
    {
        private readonly IHubContext<TradingHub> _hub;
        private readonly ScreenFieldManager _fields;

        public UiBus(IHubContext<TradingHub> hub, ScreenFieldManager fields)
        {
            _hub = hub;
            _fields = fields;
        }

        public async Task PublishStateAsync(UiState state)
        {
            // Broadcast a generic "UiState" message; the client can render it.
            await _hub.Clients.All.SendAsync("UiState", state);
        }

        public string GetField(string key) => _fields.GetField(key);
        public void SetField(string key, string value) => _fields.SetField(key, value);

        public event Func<UiEvent, Task>? OnEvent;

        public async Task EmitAsync(UiEvent evt)
        {
            var handler = OnEvent;
            if (handler != null)
            {
                // Invoke handlers sequentially; any exceptions bubble up.
                foreach (Func<UiEvent, Task> subscriber in handler.GetInvocationList())
                {
                    await subscriber(evt);
                }
            }
        }
    }
}