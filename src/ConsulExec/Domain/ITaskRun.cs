using System;

namespace ConsulExec.Domain
{
    public interface ITaskRun
    {
        string NodeName { get; }
        IObservable<string> Output { get; }
        IObservable<int> ReturnCode { get; }
    }
}