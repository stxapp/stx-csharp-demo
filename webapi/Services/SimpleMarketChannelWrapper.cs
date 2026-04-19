using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class STXMarketChannelWrapper - Wrapper around STXMarketChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimpleMarketChannelWrapper
    {
        private readonly STXMarketChannel _marketChannel;
        private Queue<STXMarketInfoChannelData> _queue { get; set; }

        public STXMarketInfoChannelData LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXMarketInfoChannelData> Items => _queue.ToList();

        public SimpleMarketChannelWrapper(STXMarketChannel marketChannel)
        {
            _queue = new Queue<STXMarketInfoChannelData>(1000);

            _marketChannel = marketChannel;
            _marketChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXMarketInfoChannelData item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _marketChannel.SetChannelAsync();

        public async Task StartAsync() => await _marketChannel.StartAsync();

        public async Task StopAsync() => await _marketChannel.StopAsync();
    }
}
