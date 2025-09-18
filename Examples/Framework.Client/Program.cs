using CoreRemoting;
using CoreRemoting.Channels.DotNetty;
using CoreRemoting.Serialization.Binary;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Framework.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new RemotingClient(new ClientConfig()
            {
                ServerHostName = "127.0.0.1",
                Serializer = new BinarySerializerAdapter(), // IMPORTANT NOTE: building with .Net Core 8 and above requires 
                MessageEncryption = false,                  // <EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>
                ServerPort = 31000,                           // to be added in your .csproj file for proper work of BinarySerializerAdapter
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

            // 👇 并发执行两个远程调用
            var task1 = Task.Run(() => proxy.GetT(dataTable));
            var task2 = Task.Run(() => proxy.GetT(dataTable1));

            var combinedTask = Task.WhenAll(task1, task2);
            var res = combinedTask.GetAwaiter().GetResult();


            Console.ReadLine();
        }
    }
}
