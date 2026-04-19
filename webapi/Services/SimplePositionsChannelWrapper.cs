using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class SimplePositionsChannelWrapper - Wrapper around STXPositionsChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimplePositionsChannelWrapper
    {
        private readonly STXPositionsChannel _positionsChannel;
        private Queue<STXPositions> _queue { get; set; }

        public STXPositions LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXPositions> Items => _queue.ToList();

        public SimplePositionsChannelWrapper(STXPositionsChannel positionsChannel)
        {
            _queue = new Queue<STXPositions>(1000);

            _positionsChannel = positionsChannel;
            _positionsChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXPositions item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _positionsChannel.SetChannelAsync();

        public async Task StartAsync() => await _positionsChannel.StartAsync();

        public async Task StopAsync() => await _positionsChannel.StopAsync();
    }
}
