using System;
using ConsulExec.Design;

namespace ConsulExec.Domain
{
    public class ConnectionOptions : ICloneable
    {
        public string Name { get; set; }

        public string ServerAddress { get; set; }

        public object Clone()
        {
            return new ConnectionOptions { Name = Name, ServerAddress = ServerAddress };
        }

        public virtual IRemoteExecution Create()
        {
            return new FakeRemoteExecution();
        }
    }
}
