using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class SimpleActiveTradesChannelWrapper - Wrapper around STXActiveTradesChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimpleActiveTradesChannelWrapper
    {
        private readonly STXActiveTradesChannel _activeTradesChannel;
        private Queue<STXActiveTrades> _queue { get; set; }

        public STXActiveTrades LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXActiveTrades> Items => _queue.ToList();

        public SimpleActiveTradesChannelWrapper(STXActiveTradesChannel activeTradesChannel)
        {
            _queue = new Queue<STXActiveTrades>(1000);

            _activeTradesChannel = activeTradesChannel;
            _activeTradesChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXActiveTrades item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _activeTradesChannel.SetChannelAsync();

        public async Task StartAsync() => await _activeTradesChannel.StartAsync();

        public async Task StopAsync() => await _activeTradesChannel.StopAsync();
    }
}
