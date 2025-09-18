// See https://aka.ms/new-console-template for more information
using CoreRemoting;
using CoreRemoting.Channels.DotNetty;
using CoreRemoting.Serialization.Binary;
using Newtonsoft.Json;
using Shared;
using System.Data;


//for (int i = 0; i < 20; i++)
//{
//    using var client1 = new RemotingClient(new ClientConfig()
//    {
//        ServerHostName = "127.0.0.1",
//        Serializer = new BinarySerializerAdapter(), // IMPORTANT NOTE: building with .Net Core 8 and above requires 
//        MessageEncryption = false,                  // <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
//        ServerPort = 9090,                           // to be added in your .csproj file for proper work of BinarySerializerAdapter
//        Channel = new DotNettyClientChannel()
//    });

//    // Establish connection to server
//    client1.Connect();

//    Console.WriteLine($"Client {i} connected.");
//}

using var client = new RemotingClient(new ClientConfig()
{
    ServerHostName = "127.0.0.1",
    Serializer = new BinarySerializerAdapter(), // IMPORTANT NOTE: building with .Net Core 8 and above requires 
    MessageEncryption = false,                  // <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
    ServerPort = 9090,                           // to be added in your .csproj file for proper work of BinarySerializerAdapter
    Channel = new DotNettyClientChannel()
});

// Establish connection to server
client.Connect();


var proxy = client.CreateProxy<IMyFirstServer>();


DataTable dataTable = new DataTable();
dataTable.Columns.Add("Name");
dataTable.Columns.Add("Age");
dataTable.Rows.Add("Mike", 25);
dataTable.Rows.Add("John", 30);
dataTable.Rows.Add("Anna", 22);


DataTable dataTable1 = new DataTable();
dataTable1.Columns.Add("names");
dataTable1.Columns.Add("ages");
dataTable1.Rows.Add("Mike", 2);
dataTable1.Rows.Add("John", 2);
dataTable1.Rows.Add("Anna", 3);

// 👇 同时发起两个异步请求
var task1 = Task.Run(() => proxy.GetT(dataTable));
var task2 = Task.Run(() => proxy.GetT(dataTable1));

// 👇 等待两个任务都完成
await Task.WhenAll(task1, task2);

// 👇 获取结果
var str = task1.Result;
var str1 = task2.Result;

Console.WriteLine(JsonConvert.SerializeObject(str));

Console.WriteLine(JsonConvert.SerializeObject(str1));
Console.ReadLine();