using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Orleans;

namespace GrainInterfaces
{
    public interface IRoomHistoryGrain : IGrain, IGrainWithGuidKey
    {
        Task<IEnumerable<Message>> GetHistory();
    }
}