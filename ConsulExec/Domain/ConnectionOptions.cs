using System;

namespace ConsulExec.Domain
{
    public class ConnectionOptions : ICloneable
    {
        public string Name { get; set; }

        public object Clone()
        {
            return new ConnectionOptions { Name = Name };
        }
    }
}
