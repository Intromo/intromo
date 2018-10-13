using Orleans;
using System;
using System.Threading.Tasks;
using HelloWorld.GrainInterfaces;

namespace HelloWorld.Grains
{
    public class Grain1 : Grain, IGreetingGrain
    {
        public Task<string> Greet(string from, string message)
        {
            return Task.FromResult($"Hi, {from}");
        }
    }
}
