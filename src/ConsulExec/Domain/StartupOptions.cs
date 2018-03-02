using System;
using System.Linq;
using System.Reactive.Linq;
using ConsulExec.Infrastructure;

namespace ConsulExec.Domain
{
    public abstract class StartupOptions : ICloneable
    {
        public string Name { get; set; } = "";

        public virtual string[] Nodes => Array.Empty<string>();

        public ConnectionOptions Connection { get; set; }

        public abstract StartupOptions Clone();

        object ICloneable.Clone() => Clone();

        public abstract IObservable<ITaskRun> Construct(string Command);

        protected void Fill(StartupOptions Other)
        {
            Name = Other.Name;
            Connection = Other.Connection;
        }
    }


    public class SequentialStartupOptions : StartupOptions
    {
        public SequentialStartupOptions(string[] Nodes)
        {
            nodes = Nodes;
        }

        public override string[] Nodes => nodes;

        public void SetNodes(string[] NewNodes)
        {
            nodes = NewNodes;
        }

        public override StartupOptions Clone()
        {
            var clone = new SequentialStartupOptions(Nodes.ToArray());
            clone.Fill(this);
            return clone;
        }

        public override IObservable<ITaskRun> Construct(string Command) =>
            Observable.Using(() => Connection.Create(),
                endpoint => HandleTasks(Command, endpoint))
            .Publish().RefCount();

        private IObservable<ITaskRun> HandleTasks(string Command, IRemoteExecution Endpoint) => Nodes.ToObservable()
            .Process(name =>
                Endpoint.Execute(
                    Observable.Return(new NodeExecutionTask(Command, NamePattern: $"^{name}$"))),
                    AllRunsAreCompleted)
            .Concat();

        private static IObservable<bool> AllRunsAreCompleted(IObservable<ITaskRun> Runs) =>
            Runs.SelectMany(v => v.ReturnCode.Select(_ => true)).All(b => b);

        private string[] nodes;
    }
}