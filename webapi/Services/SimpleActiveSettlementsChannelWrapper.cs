using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class SimpleActiveSettlementsChannelWrapper - Wrapper around STXActiveSettlementsChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimpleActiveSettlementsChannelWrapper
    {
        private readonly STXActiveSettlementsChannel _activeSettlementsChannel;
        private Queue<STXActiveSettlement> _queue { get; set; }

        public STXActiveSettlement LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXActiveSettlement> Items => _queue.ToList();

        public SimpleActiveSettlementsChannelWrapper(STXActiveSettlementsChannel activeSettlementsChannel)
        {
            _queue = new Queue<STXActiveSettlement>(1000);

            _activeSettlementsChannel = activeSettlementsChannel;
            _activeSettlementsChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXActiveSettlement item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _activeSettlementsChannel.SetChannelAsync();

        public async Task StartAsync() => await _activeSettlementsChannel.StartAsync();

        public async Task StopAsync() => await _activeSettlementsChannel.StopAsync();
    }
}
