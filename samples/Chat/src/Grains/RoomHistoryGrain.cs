using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using GrainInterfaces;
using Orleans;
using Orleans.Streams;

namespace Grains
{
    [ImplicitStreamSubscription("messages")]
    public class RoomHistoryGrain : Grain, IRoomHistoryGrain, IAsyncObserver<Message>
    {
        private readonly List<Message> _history = new List<Message>();

        public override Task OnActivateAsync()
        {
            GetStreamProvider("SMSProvider")
                .GetStream<Message>(this.GetPrimaryKey(), "messages")
                .SubscribeAsync(this);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Message>> GetHistory()
        {
            return Task.FromResult(_history.AsEnumerable());
        }

        public Task OnNextAsync(Message item, StreamSequenceToken token = null)
        {
            _history.Add(item);

            while (_history.Count > 100) {
                _history.RemoveAt(0);
            }

            return Task.CompletedTask;
        }

        public Task OnCompletedAsync() => Task.CompletedTask;

        public Task OnErrorAsync(Exception ex) => throw ex;
    }
}