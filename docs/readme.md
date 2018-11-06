# Introduction to Microsoft Orleans

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [Introduction to Microsoft Orleans](#introduction-to-microsoft-orleans)
    - [Concepts](#concepts)
        - [The Actor Model](#the-actor-model)
        - [Grains](#grains)
            - [Existence](#existence)
            - [Message Passing](#message-passing)
            - [State](#state)
    - [Samples](#samples)
        - [Hello World](#hello-world)
            - [Grains and Interfaces](#grains-and-interfaces)
            - [Silo Hosts](#silo-hosts)
            - [Cluster Clients](#cluster-clients)

<!-- /code_chunk_output -->

## Concepts

### The Actor Model
Grains are the Actors in Orlean's Virtual Actor Model paradigm. ~~If you missed it, read through [The Actor Model](#) to get a grasp on what an actor is and why they're great.~~

Grains are _actors_ - they can receive messages, act on them, and send messages to other grains.

Grains are _virtual_ - they are neither created or destroyed.

They always exist and their existence is irrespective of their locality. That's fancy talk for saying you don't need to worry about what system a grain is executing on, how it got there, or how long it's going to be there. You can simply call a method on the grain (another way of saying sending a message to) and Orleans will handle the rest.

### Grains
Grains are the fundamental computational units that get spread across a horizontal scalable cluster of worker nodes. Perhaps the simplest way of defining the behavior of a grain is to put it in terms of .NET Tasks and thread pools.

Tasks in .NET are scheduled, using a Task Scheduler, and generally executed on threads from a thread pool. This abstracts away most multithreading concerns from the developer into the underlying TPL framework. Orleans does much the same, Grains are containers for related Tasks that are scheduled and executed across *Silo* pools - your horizontally scaleable cluster.

> Note: If you're unfamiliar or need to brush up on .NETs Task Parallel Library (TPL) - have a read through the [TPL Pitstop Chapter](#)

Grains are defined as strongly-typed interfaces extending from special grain interfaces. These interfaces define both the type of the grain's identifier(s) and the Tasks the grain fulfills. Additionally, attributes can be applied to the interface and methods to alter the behavior of the grain and Task handling.

Here's a quick example:

```c#
public interface IReceptorGrain : IGrainWithGuidKey
{
    Task Innervate(INeurotransmitter transmitter);
}
```

We've defined a grain extending from `IGrainWithGuidKey`. This means we'll use a `Guid` to interact with instances of this grain. The `IReceptorGrain` models a neural receptor and accepts one message, `Innervate(...)`, which takes in one parameter. You'll also notice that the method returns a plain `Task` without any return type. All grain methods are required to return promises so Orleans can manage grain interactions asynchronously.

>All grain methods are required to return `Task` or `Task<...>`.

>Tasks are promises that for an operation, in the future, that operation may complete. There are special tasks, `Task<...>`, that function the same but also allow the completed asyncronous operation to return a result as a part of the promise. Tasks generally transition between "WaitingForActivation", "Running", and "RanToCompletion" states but they can also be used to indicate error cases.

#### Existence

#### Message Passing

#### State

## Samples

### Hello World

First, we'll create a directory for our projects.

```bash
mkdir -p ~/workspace
cd ~/workspace
```

Let's create a new solution for our hello world application. Orleans provides a convenient dotnet template for setting up a solution, let's first add that to our dotnet CLI tool.

```bash
dotnet new --install Microsoft.Orleans.Templates::*
```

Now we can setup our project:

```bash
dotnet new orleans -n HellowWorld
```

Orleans uses a general four part structure for projects:
- Grain Interfaces
- Grain Implementations
- Client application
- Host (or server) application

You'll see corresponding directories in **src/** for each of these. **GrainInterfaces** and **Grains** contains .NET Standard library projects, the other two projects are .NET Core 2.0 application projects. The Orleans template did a fair amount behind the scenes to help you get started, here's what you can do immediately:

- Open the solution in VS Code, Visual Studio, or any editor
- Build the solution using the above or `dotnet build`
- Easily debug the solution using VS Code or Visual Studio

#### Grains and Interfaces
Grains are the actors in Orlean's virtual actor model. They're virtual because their activation and existence is abstracted away from their function.

>Read more about Actors in the [Actor Model](#).

Let's add a simple greeting grain that accepts a greeting message and responds in kind.

**src/GrainInterfaces/IGreetingGrain.cs**
```c#
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
```

Grain interface define the messages an actor responds to. In our `IGreetingGrain` interface we've told Orleans that we'll respond to `Greet(string, string)` messages.

You'll note two things with this example:
- We've extended from `Orleans.IGrain`
- Our Greet method returns a `Task<string>`

>Orleans employs code-generation to vastly reduce the amount of code developers need to write. The `IGrain` interface is used by the framework for this generation as well as for various abstracted aspects of the infrastructure.
>
>All grain methods must return `Task<string>`. This allows Orleans to leverage async/await for asynchronous turn management.

Here's a simple implementation for our greeting interface.

>Orleans allows for multiple implementations of a given grain interface but in practice you'll normally only use one.

**src/Grains/GreetingGrain.cs**
```c#
using System.Threading.Tasks;
using Interfaces;
using Orleans;

namespace Grains
{
    public class GreetingGrain : Grain, IGreetingGrain
    {
        public Task<string> Greet(string from, string message)
        {
            return Task.FromResult($"Hi, {from}");
        }
    }
}
```

It should all seem pretty straight forward. We implement the `Greet` method and return "Hi!". If you're not familiar with [TPL](#), `Task.FromResult()` wraps the result in an awaitable `Task<string>`. This can be avoided by marking the `Greet` method with the `async` qualifier.

>If you do mark the method as async, you'll likely get a warning due to not *awaiting*. Read more about our feelings on this warning with regards to Orleans grains in [Debugging](#).

#### Silo Hosts

Orleans calls the services that manage grains and execute logic on their behalf "Silos". For now, the dotnet CLI Orleans template has taken care of our silo code for us. Skip ahead to [Silos and Clients](#) if you want to learn more now.

#### Cluster Clients

Cluster Clients are clients to Orleans Silos. They're, generally, the ones calling and using grains.

Let's update the client generated for us to invoke our new grain.

**src/ClusterClient/Program.cs**
```c#
// ...

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

// ...
```

Above, you'll notice we're calling `IClusterClient.GetGrain<IGreetingGrain>(...)`. This gives us a *reference* to our greeting grain of the interface, `IGreetingGrain`.  This reference is a shallow object that isn't actually tied to a silo just yet. When we call a method on the reference then the magic happens. The request gets funneled to a Silo, either a new one which activates the grain for the first time, or the Silo that has the currently activated grain, and the method is executed.

How is one grain distinguished from the other? That's the purpose of  `Guid.NewGuid()` being passed to `GetGrain(...)`. It is the primary key for the grain we're calling. If we use that same Guid later we'll get the same grain back.

How does our grain reference know which Silo to send the grain call to? The ClusterClient is connected to a single Silo and issues requests through that Silo. Any request that comes in destined for a grain at a different Silo is forwarded on to the owning Silo. If an owning Silo isn't known than the request is forwarded to a Silo until it is found. Each Silo attempts to cache a directory of activations to minimize this chatter as much as possible.

>That was a mouthful! If you're feeling overwhelmed, that's ok, that's as much as we'll cover on that topic for now. If you want to learn more, skip to [Grain Activation](#).

Build and run and you'll see this:

```bash
Client successfully connected to silo host
Who are you: Austin
Enter a greeting: Hello
Received: Hi, Austin
```

