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

        public abstract StartupOptions Clone();

        object ICloneable.Clone() => Clone();
        public abstract IObservable<ITaskRun> Construct(IRemoteExecution ExecuteService, string Command);
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

        public override StartupOptions Clone() => new SequentialStartupOptions(Nodes.ToArray()) { Name = Name };

        public override IObservable<ITaskRun> Construct(IRemoteExecution ExecuteService, string Command) =>
            Nodes.ToObservable()
            .Process(name => ExecuteService.Execute(Observable.Return(new NodeExecutionTask(Command, NamePattern: $"^{name}$"))),
                    t => t.SelectMany(v => v.ReturnCode.Select(_ => true)).All(b => b))
            .Concat().Publish().RefCount();

        private string[] nodes;
    }
}