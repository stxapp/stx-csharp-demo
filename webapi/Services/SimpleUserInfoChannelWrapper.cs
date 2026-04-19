using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class SimpleUserInfoChannelWrapper - Wrapper around STXUserInfoChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimpleUserInfoChannelWrapper
    {
        private readonly STXUserInfoChannel _userInfoChannel;
        private Queue<STXProfileDetails> _queue { get; set; }

        public STXProfileDetails LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXProfileDetails> Items => _queue.ToList();

        public SimpleUserInfoChannelWrapper(STXUserInfoChannel userInfoChannel)
        {
            _queue = new Queue<STXProfileDetails>(1000);

            _userInfoChannel = userInfoChannel;
            _userInfoChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXProfileDetails item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _userInfoChannel.SetChannelAsync();

        public async Task StartAsync() => await _userInfoChannel.StartAsync();

        public async Task StopAsync() => await _userInfoChannel.StopAsync();
    }
}
