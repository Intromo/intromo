# Introduction to Microsoft Orleans

<!-- @import "[TOC]" {cmd="toc" depthFrom=1 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [Introduction to Microsoft Orleans](#introduction-to-microsoft-orleans)
    - [Samples](#samples)
        - [Hello World](#hello-world)
            - [Grains and Interfaces](#grains-and-interfaces)
            - [Silo Hosts](#silo-hosts)
            - [Cluster Clients](#cluster-clients)

<!-- /code_chunk_output -->

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

<script src="http://gist-it.appspot.com/https://github.com/berdon/intromo/raw/master/samples/HelloWorld/src/GrainInterfaces/IGreetingGrain.cs"></script>

Grain interface define the messages an actor responds to. In our `IGreetingGrain` interface we've told Orleans that we'll respond to `Greet(string, string)` messages.

You'll note two things with this example:
- We've extended from `Orleans.IGrain`
- Our Greet method returns a `Task<string>`

>Orleans employs code-generation to vastly reduce the amount of code developers need to write. The `IGrain` interface is used by the framework for this generation as well as for various abstracted aspects of the infrastructure.
>
>All grain methods must return `Task<string>`. This allows Orleans to leverage async/await for asynchronous turn management.

Here's a simple implementation for our greeting interface.

>Orleans allows for multiple implementations of a given grain interface but in practice you'll normally only use one.

<script src="http://gist-it.appspot.com/https://github.com/berdon/intromo/raw/master/samples/HelloWorld/src/Grains/GreetingGrain.cs"></script>

It should all seem pretty straight forward. We implement the `Greet` method and return "Hi!". If you're not familiar with [TPL](#), `Task.FromResult()` wraps the result in an awaitable `Task<string>`. This can be avoided by marking the `Greet` method with the `async` qualifier.

>If you do mark the method as async, you'll likely get a warning due to not *awaiting*. Read more about our feelings on this warning with regards to Orleans grains in [Debugging](#).

#### Silo Hosts

Orleans calls the services that manage grains and execute logic on their behalf "Silos". For now, the dotnet CLI Orleans template has taken care of our silo code for us. Skip ahead to [Silos and Clients](#) if you want to learn more now.

#### Cluster Clients

Cluster Clients are clients to Orleans Silos. They're, generally, the ones calling and using grains.

Let's update the client generated for us to invoke our new grain.

<script src="http://gist-it.appspot.com/https://github.com/berdon/intromo/raw/master/samples/HelloWorld/src/ClusterClient/Program.cs?slice=76:88"></script>

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

