// See https://aka.ms/new-console-template for more information
using Core.Server;
using CoreRemoting;
using CoreRemoting.Channels.DotNetty;
using CoreRemoting.DependencyInjection;
using CoreRemoting.Serialization.Binary;
using Shared;

using var server = new RemotingServer(new ServerConfig()
{
    HostName = "0.0.0.0",
    NetworkPort = 9090,
    MessageEncryption = false,
    Serializer = new BinarySerializerAdapter(), // IMPORTANT NOTE: building with .Net Core 8 and above requires 
                                                // <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
                                                // to be added in your .csproj file for proper work of BinarySerializerAdapter
    RegisterServicesAction = container =>
    {
        container.RegisterService<IMyFirstServer, MyFirstServer>(ServiceLifetime.Singleton);
    },
    Channel = new DotNettyServerChannel()
});


server.Error += (sender, exception) =>
{
    Console.WriteLine("--[Error]--------------------------");
    Console.WriteLine(exception.Message);
    Console.WriteLine(exception.StackTrace);

    if (exception.InnerException != null)
    {
        Console.WriteLine(exception.InnerException.Message);
        Console.WriteLine(exception.InnerException.StackTrace);
    }

    Console.WriteLine("-----------------------------------");
};

// Start server
server.Start();

Console.WriteLine("\nRegistered services");
Console.WriteLine("-------------------");

// List registered services
foreach (var registration in server.ServiceRegistry.GetServiceRegistrations())
{
    Console.WriteLine($"ServiceName = '{registration.ServiceName}', InterfaceType = {registration.InterfaceType.FullName}, UsesFactory = {registration.UsesFactory}, Lifetime = {registration.ServiceLifetime}");
}

Console.WriteLine("\nServer is running.");
Console.ReadLine();