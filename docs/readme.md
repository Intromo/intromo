# Introduction to Microsoft Orleans

## Samples

### Hello World

Let's create a new solution for our hello world application.

```bash
mkdir -p ~/workspace
cd ~/workspace

mkdir hello-world
cd hello-world
dotnet new sln -n hello-world
```

Orleans uses a general four part structure for projects:
- Grain Interfaces
- Grain Implementations
- Client application
- Host (or server) application

Let's add these projects to the solution.
```bash
dotnet new classlib -n Interfaces
dotnet new classlib -n Grains
dotnet new console -n Client
dotnet new console -n Host
dotnet sln add **/*.csproj 

rm -rf **/Class1.cs
```

Above, we created two class library projects, two console projects, and then we added the projects to the solution. You'll note that we removed some boilerplate classes that get created - just general house cleaning.

For each project, we'll want to add certain Orleans nuget packages. These are as follows:

- Grain interfaces and implementations
    - Microsoft.Orleans.Core.Abstractions
    - Microsoft.Orleans.CodeGenerator.MSBuild
- Client project
    - Microsoft.Orleans.Client
- Host project
    - Microsoft.Orleans.Server

Let's add these packages to each project.

```bash
cd interfaces
dotnet add package Microsoft.Orleans.Core.Abstractions
dotnet add package Microsoft.Orleans.CodeGenerator.MSBuild

cd ../grains
dotnet add package Microsoft.Orleans.Core.Abstractions
dotnet add package Microsoft.Orleans.CodeGenerator.MSBuild

cd ../client
dotnet add package Microsoft.Orleans.Client

cd ../host
dotnet add package Microsoft.Orleans.Server
```

Finally, let's reference our class libraries where needed.

```bash
cd ../grains
dotnet add reference ../interfaces/interfaces.csproj 

cd ../client
dotnet add reference ../interfaces/interfaces.csproj

cd ../host
dotnet add reference ../interfaces/interfaces.csproj
dotnet add reference ../grains/grains.csproj
```
#### Grains
Grains are the actors in Orlean's virtual actor model. They're virtual because their activation and existence is abstracted away from their function.

>Read more about Actors in the [Actor Model](#).

Let's add a simple greeting grain that accepts a greeting message and responds in kind.

**IGreetingGrain.cs**
```c#
using System.Threading.Tasks;
using Orleans;

namespace Interfaces
{
    public interface IGreetingGrain : IGrain
    {
         Task<string> Greet(string message);
    }
}
```

Grain interface define the messages an actor responds to. In our `IGreetingGrain` interface we've told Orleans that we'll respond to `Greet(string)` messages.

You'll note two things with this example:
- We've extended from `Orleans.IGrain`
- Our Greet method returns a `Task<string>`

>Orleans employs code-generation to vastly reduce the amount of code developers need to write. The `IGrain` interface is used by the framework for this generation as well as for various abstracted aspects of the infrastructure.
>
>All grain methods must return `Task<string>`. This allows Orleans to leverage async/await for asynchronous turn management.

Here's a simple implementation for our greeting interface.

>Orleans allows for multiple implementations of a given grain interface but in practice you'll normally only use one.

```c#
using System.Threading.Tasks;
using Interfaces;
using Orleans;

namespace Grains
{
    public class GreetingGrain : Grain, IGreetingGrain
    {
        public Task<string> Greet(string message)
        {
            return Task.FromResult("Hi!");
        }
    }
}
```

It should all seem pretty straight forward. We implement the `Greet` method and return "Hi!". If you're not familiar with [TPL](#), `Task.FromResult()` wraps the result in an awaitable `Task<string>`. This can be avoided by marking the `Greet` method with the `async` qualifier.

>If you do mark the method as async, you'll likely get a warning due to not *awaiting*. Read more about our feelings on this warning with regards to Orleans grains in [Debugging](#).

