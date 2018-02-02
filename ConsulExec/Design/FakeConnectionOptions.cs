using ConsulExec.Domain;

namespace ConsulExec.Design
{
    internal class FakeConnectionOptions : ConnectionOptions
    {
        private FakeRemoteExecution instance;

        public override IRemoteExecution Create() => 
            instance ?? (instance = new FakeRemoteExecution(ServerAddress));

        public override object Clone() =>
            new FakeConnectionOptions { Name = Name, ServerAddress = ServerAddress };
    }
}