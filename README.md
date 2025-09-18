# CoreRemoting.DotNetty

## Language / 语言选择

[**English**](#english-version) | [中文](#chinese-version)

---

## English Version

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
- Newtonsoft.Json
- DotNetty.Transport

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

---

## Chinese Version

基于 [CoreRemoting](https://github.com/theRainbird/CoreRemoting.git) 扩展的、使用 [DotNetty](https://github.com/Azure/DotNetty) 作为网络传输层的 RPC 通信库。

### 项目简介

CoreRemoting.DotNetty 是对 CoreRemoting 库的扩展实现，使用高性能的 DotNetty 网络框架替代了原有的 WatsonTcp 实现，提供更强大的网络通信能力和更灵活的配置选项。

### 主要特性

- 完全兼容 CoreRemoting 的 API 和功能
- 基于 DotNetty 的高性能异步网络通信
- 支持跨框架序列化（.NET Framework 和 .NET Core/.NET 5+/ .NET 8）
- 支持双向 RPC 调用和事件传递
- 提供内置的加密和认证机制
- 支持自定义序列化器（默认使用 Bson，也支持 BinaryFormatter）
- 提供类似于传统 .NET Remoting 的编程体验

### 项目结构

```
├── CoreRemoting.Channels\          # 核心实现代码
│   ├── Channels\
│   │   ├── DotNetty\              # DotNetty 通道实现
│   │   ├── IClientChannel.cs       # 客户端通道接口
│   │   └── IServerChannel.cs       # 服务器通道接口
│   ├── Serialization\             # 序列化相关实现
│   └── ...
├── Examples\                       # 示例代码
│   ├── Core.Client\               # .NET Core 客户端示例
│   ├── Core.Server\               # .NET Core 服务器示例
│   ├── Framework.Client\          # .NET Framework 客户端示例
│   └── Shared\                    # 共享接口定义
├── CoreRemoting.DotNetty.sln       # 解决方案文件
```

### 使用方法

#### 安装

通过 NuGet 安装相关包：
- CoreRemoting.Channels.DotNetty
- Newtonsoft.Json
- DotNetty.Transport

#### 服务器端代码示例

```csharp
using CoreRemoting;
using CoreRemoting.Channels.DotNetty;

// 创建并配置服务器
var server = new RemotingServer(new ServerConfig()
{
    HostName = "localhost",
    NetworkPort = 9090,
    // 使用 DotNetty 通道
    ChannelType = typeof(DotNettyServerChannel),
    // 注册服务
    RegisterServicesAction = container =>
    {
        container.RegisterService<IMyService, MyServiceImpl>(ServiceLifetime.Singleton);
    }
});

// 启动服务器
server.Start();

Console.WriteLine("服务器已启动，按任意键停止...");
Console.ReadLine();

// 停止服务器
server.Shutdown();
```

#### 客户端代码示例

```csharp
using CoreRemoting;
using CoreRemoting.Channels.DotNetty;

// 创建并配置客户端
var client = new RemotingClient(new ClientConfig()
{
    ServerHostName = "localhost",
    ServerPort = 9090,
    // 使用 DotNetty 通道
    ChannelType = typeof(DotNettyClientChannel)
});

// 连接到服务器
client.Connect();

// 创建远程服务代理
var proxy = client.CreateProxy<IMyService>();

// 调用远程方法
var result = proxy.MyMethod(parameters);

Console.WriteLine("调用结果: " + result);

// 断开连接
client.Disconnect();
```

#### 跨框架序列化支持

CoreRemoting.DotNetty 支持在 .NET Framework 和 .NET Core/.NET 5+/.NET 8 之间进行序列化和反序列化。

```csharp
// 在 .NET Framework 客户端连接到 .NET Core 服务器时
CrossFrameworkSerialization.RedirectPrivateCoreLibToMscorlib();

// 或者在 .NET Core 客户端连接到 .NET Framework 服务器时
CrossFrameworkSerialization.RedirectMscorlibToPrivateCoreLib();
```

### 示例项目

项目包含多个示例，展示了不同场景下的使用方法：
- Core.Server：基于 .NET 8 的服务器示例
- Core.Client：基于 .NET 8 的客户端示例
- Framework.Client：基于 .NET Framework 的客户端示例
- Shared：包含所有示例共享的服务接口定义

### 依赖项

- [CoreRemoting](https://github.com/theRainbird/CoreRemoting.git)
- [DotNetty](https://github.com/Azure/DotNetty)
- Newtonsoft.Json
- Castle.DynamicProxy

### 注意事项

1. 在使用跨框架序列化时，请确保在应用程序启动时调用相应的重定向方法
2. 对于大数据传输，请调整 DotNetty 的缓冲区设置
3. 在生产环境中，请配置适当的安全设置和认证机制
4. 处理异步操作时，请注意避免死锁

### 许可证

本项目使用与 CoreRemoting 和 DotNetty 相同的许可证。