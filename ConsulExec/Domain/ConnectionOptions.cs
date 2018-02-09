using System;

namespace ConsulExec.Domain
{
    public class ConnectionOptions : ICloneable
    {
        public string Name { get; set; }

        public string ServerAddress { get; set; }

        public virtual object Clone() =>
            new ConnectionOptions { Name = Name, ServerAddress = ServerAddress };

        public virtual IRemoteExecution Create() =>
            new RemoteExecution(ServerAddress);
    }
}
