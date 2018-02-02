using System;
using System.Reactive.Linq;
using ConsulExec.Domain;

namespace ConsulExec.Design
{
    public class FakeTaskRun : ITaskRun
    {
        public static FakeTaskRun Create(NodeExecutionTask Task, string Server = "")
        {
            var heartbeat = Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(10).Publish().RefCount();
            var nodeName = Task.NamePattern.Substring(1, Task.NamePattern.Length - 2);
            var server = string.IsNullOrEmpty(Server) ? "" : $" via {Server}";
            return new FakeTaskRun
            {
                Task = Task,
                NodeName = nodeName,
                Output = heartbeat.Select(v => v > 2 ? "" : $"{nodeName} executing {Task.Command}{server} output: {v}\n"),
                ReturnCode = heartbeat.LastAsync().Select(_ => 42)
            };
        }
        public NodeExecutionTask Task { get; set; }
        public string NodeName { get; set; }
        public IObservable<string> Output { get; set; }
        public IObservable<int> ReturnCode { get; set; }
    }
}