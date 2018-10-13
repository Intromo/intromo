using Orleans;
using System;
using System.Threading.Tasks;

namespace HelloWorld.GrainInterfaces
{
    public interface IGreetingGrain : IGrainWithGuidKey
    {
        Task<string> Greet(string from, string message);
    }
}
