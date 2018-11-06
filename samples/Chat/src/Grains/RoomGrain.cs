using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using GrainInterfaces;
using Orleans;

namespace Grains
{
    public class RoomGrain : Grain, IRoomGrain
    {
        private readonly Dictionary<Guid, User> _users = new Dictionary<Guid, User>();

        public Task Join(User user)
        {
            _users.Add(user.Id, user);
            return Task.CompletedTask;
        }

        public Task Leave(Guid userId)
        {
            _users.Remove(userId);
            return Task.CompletedTask;
        }

        public Task SendMessage(Message message)
        {
            if (!_users.TryGetValue(message.FromId, out var user))
            {
                throw new Exception("Invalid from user ID");
            }

            message.From = user.Name;

            return GetStreamProvider("SMSProvider")
                .GetStream<Message>(this.GetPrimaryKey(), "messages")
                .OnNextAsync(message);
        }
    }
}