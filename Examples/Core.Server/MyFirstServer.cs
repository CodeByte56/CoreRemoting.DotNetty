using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Server
{
    public class MyFirstServer : IMyFirstServer
    {
        public T GetT<T>(T ent)
        {
            return ent;
        }
    }
}
