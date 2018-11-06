using System;
using System.Threading.Tasks;
using Domain;
using Orleans;

namespace GrainInterfaces
{
    public interface IRoomGrain : IGrain, IGrainWithGuidKey
    {
        Task Join(User user);
        Task Leave(Guid userId);
        Task SendMessage(Message message);
    }
}