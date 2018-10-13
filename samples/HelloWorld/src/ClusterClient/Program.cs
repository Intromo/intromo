using HelloWorld.GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using System;
using System.Threading.Tasks;

namespace HelloWorld.ClusterClient
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    await DoClientWork(client);

                    Console.ReadKey();
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                return 1;
            }
        }

        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            var attempt = 0;
            var client = default(IClusterClient);

            while (true)
            {
                try
                {
                    client = new ClientBuilder()
                        .UseLocalhostClustering()
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "HelloWorldApp";
                        })
                        .ConfigureLogging(logging => logging.AddConsole())
                        .Build();

                    await client.Connect();

                    Console.WriteLine("Client successfully connected to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;

                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");

                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }

            return client;
        }

        private static async Task DoClientWork(IClusterClient client)
        {
            Console.Write("Who are you: ");
            var from = Console.ReadLine();
            Console.Write("Enter a greeting: ");
            var message = Console.ReadLine();

            var grain = client.GetGrain<IGreetingGrain>(Guid.NewGuid());
            var result = await grain.Greet(from, message);

            Console.WriteLine($"Received: {result}");
        }
    }
}
