using System;

namespace ConsulExec.Domain
{
    public interface IRemoteExecution : IDisposable
    {
        IObservable<string[]> Nodes { get; }
        IObservable<ITaskRun> Execute(IObservable<NodeExecutionTask> Tasks);
    }
}