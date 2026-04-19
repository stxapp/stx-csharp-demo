using STX.Sdk.Channels;
using STX.Sdk.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Services
{
    /// <summary>
    /// Class SimplePortfolioChannelWrapper - Wrapper around STXPortfolioChannel.
    /// Simple implementation with Queue - when item is received it's Enqueued to Queue. 
    /// Supports methods LastItem (returns last added item to Queue) and Items (all items from Queue).
    /// </summary>
    public class SimplePortfolioChannelWrapper
    {
        private readonly STXPortfolioChannel _portfolioChannel;
        private Queue<STXPortfolio> _queue { get; set; }

        public STXPortfolio LastItem => _queue.Any() ? _queue.Last() : null;
        public List<STXPortfolio> Items => _queue.ToList();

        public SimplePortfolioChannelWrapper(STXPortfolioChannel portfolioChannel)
        {
            _queue = new Queue<STXPortfolio>(1000);

            _portfolioChannel = portfolioChannel;
            _portfolioChannel.SetOnReceiveAction(OnReceiveToDo);
        }

        public void OnReceiveToDo(STXPortfolio item)
        {
            _queue.Enqueue(item);
        }

        public async Task SetChannelAsync() => await _portfolioChannel.SetChannelAsync();

        public async Task StartAsync() => await _portfolioChannel.StartAsync();

        public async Task StopAsync() => await _portfolioChannel.StopAsync();
    }
}
