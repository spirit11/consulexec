using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ConsulExec.Domain;
using ConsulExec.Infrastructure;

namespace ConsulExec.Design
{
    internal class FakeRemoteExecution : IRemoteExecution
    {
        public FakeRemoteExecution(string Address)
        {
            address = Address;
            Nodes = Observable.Create<string[]>(async (o, ct) =>
            {
                var idx = 0;
                var nodes = new List<string> { "blinking" };
                while (!ct.IsCancellationRequested)
                {
                    Debug.WriteLine("FakeRemoteExecution: requesting nodes");

                    await Task.Delay(100, ct);

                    nodes.Add(idx++.ToString());
                    if (idx % 3 == 0)
                        if (!nodes.Remove("blinking"))
                            nodes.Add("blinking");
                    if (idx % 8 == 0)
                        throw new Exception("Transient exception");

                    Debug.WriteLine("FakeRemoteExecution: request completed");
                    o.OnNext(nodes.ToArray());
                    try
                    {
                        await Task.Delay(200, ct);
                    }
                    catch (Exception)
                    {
                        break;
                        //throw;
                    }
                }
            }).Retry(TimeSpan.FromSeconds(2)).Publish().RefCount();
        }

        public IObservable<string[]> Nodes { get; }

        public IObservable<ITaskRun> Execute(IObservable<NodeExecutionTask> Tasks) =>
            Tasks.Select(t => FakeTaskRun.Create(t, address));

        public void Dispose()
        {
            Debug.WriteLine("FakeRemoteExecution: disposed");
        }

        private readonly string address;
    }
}
