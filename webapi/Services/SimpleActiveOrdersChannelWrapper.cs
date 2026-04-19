using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class SimpleActiveOrdersChannelWrapper - Wrapper around STXActiveOrdersChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimpleActiveOrdersChannelWrapper
    {
        private readonly STXActiveOrdersChannel _activeOrdersChannel;
        private Queue<STXActiveOrders> _queue { get; set; }

        public STXActiveOrders LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXActiveOrders> Items => _queue.ToList();

        public SimpleActiveOrdersChannelWrapper(STXActiveOrdersChannel activeOrdersChannel)
        {
            _queue = new Queue<STXActiveOrders>(1000);

            _activeOrdersChannel = activeOrdersChannel;
            _activeOrdersChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXActiveOrders item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _activeOrdersChannel.SetChannelAsync();

        public async Task StartAsync() => await _activeOrdersChannel.StartAsync();

        public async Task StopAsync() => await _activeOrdersChannel.StopAsync();
    }
}
