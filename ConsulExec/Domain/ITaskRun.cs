using System;

namespace ConsulExec.Domain
{
    public interface ITaskRun
    {
        NodeExecutionTask Task { get; }
        string NodeName { get; }
        IObservable<string> Output { get; }
        IObservable<int> ReturnCode { get; }
    }
}