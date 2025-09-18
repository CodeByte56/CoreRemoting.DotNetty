# CoreRemoting.Channels.DotNetty

## Language
[View Chinese Version](README.zh-CN.md)

---

An extension of [CoreRemoting](https://github.com/theRainbird/CoreRemoting.git) using [DotNetty](https://github.com/Azure/DotNetty) as the network transport layer for RPC communication.

### Project Introduction

CoreRemoting.DotNetty is an extended implementation of the CoreRemoting library that replaces the original WatsonTcp implementation with the high-performance DotNetty network framework, providing more powerful network communication capabilities and more flexible configuration options.

### Key Features

- Fully compatible with CoreRemoting APIs and functionality
- High-performance asynchronous network communication based on DotNetty
- Support for cross-framework serialization (.NET Framework and .NET Core/.NET 5+/ .NET 8)
- Support for bidirectional RPC calls and event delivery
- Built-in encryption and authentication mechanisms
- Support for custom serializers (Bson by default, also supports BinaryFormatter)
- Provides programming experience similar to traditional .NET Remoting

### Project Structure

```
├── CoreRemoting.Channels\          # Core implementation code
│   ├── Channels\
│   │   ├── DotNetty\              # DotNetty channel implementation
│   │   ├── IClientChannel.cs       # Client channel interface
│   │   └── IServerChannel.cs       # Server channel interface
│   ├── Serialization\             # Serialization related implementations
│   └── ...
├── Examples\                       # Example code
│   ├── Core.Client\               # .NET Core client example
│   ├── Core.Server\               # .NET Core server example
│   ├── Framework.Client\          # .NET Framework client example
│   └── Shared\                    # Shared interface definitions
├── CoreRemoting.DotNetty.sln       # Solution file
```

### Usage

#### Installation

Install the relevant packages via NuGet:
- CoreRemoting.Channels.DotNetty

#### Server-side Code Example

```csharp
using CoreRemoting;
using CoreRemoting.Channels.DotNetty;

// Create and configure the server
var server = new RemotingServer(new ServerConfig()
{
    HostName = "localhost",
    NetworkPort = 9090,
    // Use DotNetty channel
    ChannelType = typeof(DotNettyServerChannel),
    // Register services
    RegisterServicesAction = container =>
    {
        container.RegisterService<IMyService, MyServiceImpl>(ServiceLifetime.Singleton);
    }
});

// Start the server
server.Start();

Console.WriteLine("Server started, press any key to stop...");
Console.ReadLine();

// Stop the server
server.Shutdown();
```

#### Client-side Code Example

```csharp
using CoreRemoting;
using CoreRemoting.Channels.DotNetty;

// Create and configure the client
var client = new RemotingClient(new ClientConfig()
{
    ServerHostName = "localhost",
    ServerPort = 9090,
    // Use DotNetty channel
    ChannelType = typeof(DotNettyClientChannel)
});

// Connect to the server
client.Connect();

// Create a remote service proxy
var proxy = client.CreateProxy<IMyService>();

// Call remote method
var result = proxy.MyMethod(parameters);

Console.WriteLine("Call result: " + result);

// Disconnect
client.Disconnect();
```

#### Cross-framework Serialization Support

CoreRemoting.DotNetty supports serialization and deserialization between .NET Framework and .NET Core/.NET 5+/ .NET 8.

```csharp
// When .NET Framework client connects to .NET Core server
CrossFrameworkSerialization.RedirectPrivateCoreLibToMscorlib();

// Or when .NET Core client connects to .NET Framework server
CrossFrameworkSerialization.RedirectMscorlibToPrivateCoreLib();
```

### Example Projects

The project includes several examples demonstrating usage in different scenarios:
- Core.Server: .NET 8-based server example
- Core.Client: .NET 8-based client example
- Framework.Client: .NET Framework-based client example
- Shared: Contains service interface definitions shared by all examples

### Dependencies

- [CoreRemoting](https://github.com/theRainbird/CoreRemoting.git)
- [DotNetty](https://github.com/Azure/DotNetty)
- Newtonsoft.Json
- Castle.DynamicProxy

### Notes

1. When using cross-framework serialization, ensure to call the corresponding redirection method at application startup
2. For large data transfers, adjust DotNetty's buffer settings
3. In production environments, configure appropriate security settings and authentication mechanisms
4. When handling asynchronous operations, be careful to avoid deadlocks

### License

This project uses the same license as CoreRemoting and DotNetty.


## Language / 语言选择

This is the English version of the README. For the Chinese version, please see [View Chinese Version](README.zh-CN.md).