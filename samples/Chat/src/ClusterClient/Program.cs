using Domain;
using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat.ClusterClient
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
                        .AddSimpleMessageStreamProvider("SMSProvider")
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
            Console.Write("Enter your name: ");
            var name = Console.ReadLine();
            var user = new User(name);

            var roomSubscriptions = new Dictionary<string, StreamSubscriptionHandle<Message>>();
            roomSubscriptions.Add("general", await SubscribeToRoom(client, user, "general"));
            var activeRoom = "general";

            string input = null;
            
            while (true)
            {
                Console.Write("> ");
                input = Console.ReadLine();

                if (input == "/" || input == "/quit")
                {
                    foreach (var kvp in roomSubscriptions)
                    {
                        await client.GetGrain<IRoomGrain>(Room.GetRoomId(kvp.Key)).Leave(user.Id);
                        await kvp.Value.UnsubscribeAsync();
                    }

                    Console.WriteLine("# Seeya");
                    break;
                }
                else if (input.StartsWith("/join"))
                {
                    var roomName = input.Split(' ')[1];

                    Console.WriteLine($"# Joining '{roomName}'.");
                    
                    roomSubscriptions.Add(roomName, await SubscribeToRoom(client, user, roomName));
                }
                else if (input == "/list")
                {
                    foreach (var room in roomSubscriptions.Keys)
                    {
                        Console.WriteLine(room);
                    }
                }
                else if (input.StartsWith("/focus"))
                {
                    var roomName = input.Split(' ')[1];
                    if (!roomSubscriptions.ContainsKey(roomName))
                    {
                        Console.WriteLine($"You're not in that room, use /join {roomName} to join it.");
                    }
                    else
                    {
                        activeRoom = roomName;
                    }
                }
                else
                {
                    await client.GetGrain<IRoomGrain>(Room.GetRoomId(activeRoom)).SendMessage(new Message
                    {
                        FromId = user.Id,
                        Body = input
                    });
                }
            }
        }

        private static async Task<StreamSubscriptionHandle<Message>> SubscribeToRoom(IClusterClient client, User user, string roomName)
        {
            var streamProvider = client.GetStreamProvider("SMSProvider");

            var roomId = Room.GetRoomId(roomName);

            var roomGrain = client.GetGrain<IRoomGrain>(roomId);
            await roomGrain.Join(user);

            async Task handleMessages(Message message, StreamSequenceToken sequenceId)
            {
                if (message.FromId == user.Id)
                {
                    Console.WriteLine($"\r[{message.PublishedAt:hh:mm:ss}] You say, \"{message.Body}\"");
                }
                else
                {
                    Console.WriteLine("\r" + message);
                    Console.Write("> ");
                }
            }

            var roomHistory = await client.GetGrain<IRoomHistoryGrain>(roomId).GetHistory();
            foreach (var message in roomHistory)
            {
                await handleMessages(message, null);
            }

            var stream = streamProvider.GetStream<Message>(roomId, "messages");
            return await stream.SubscribeAsync<Message>(handleMessages);
        }
    }
}
