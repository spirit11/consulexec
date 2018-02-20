using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using ConsulExec.Infrastructure;

namespace ConsulExec.Domain
{
    public class RemoteExecution : IRemoteExecution
    {
        public RemoteExecution(string ServerAddress = "http://localhost:8500")
        {
            address = new Uri(ServerAddress);
            client = new Lazy<ConsulClient>(() => new ConsulClient(config => { config.Address = address; }), LazyThreadSafetyMode.ExecutionAndPublication);

            Nodes = Observable.Create<string[]>(async (o, ct) =>
            {
                using (var client = await CreateClientAsync())
                {
                    while (!ct.IsCancellationRequested)
                    {
                        Debug.WriteLine($"requesting nodes from {address}");
                        QueryResult<AgentMember[]> nodes;
                        try
                        {
                            nodes = await client.Agent.Members(false, ct);
                        }
                        catch (Exception)
                        {
                            o.OnNext(Array.Empty<string>());
                            throw;
                        }
                        Debug.WriteLine($"request to {address} completed");
                        /*
                     
1057		status := []MemberStatus{StatusNone, StatusAlive, StatusLeaving, StatusLeft, StatusFailed}
1058		expect := []string{"none", "alive", "leaving", "left", "failed"}
                     */
                        //nodes.Response.First().Status
                        if (nodes.Response != null) // omg, it can be null !
                            o.OnNext(nodes.Response.Where(v => v.Status == 1).Select(v => v.Name).ToArray());
                        await Task.Delay(1000, ct);
                    }
                }
            }).Retry(TimeSpan.FromSeconds(2)).Publish().RefCount();
        }

        public IObservable<string[]> Nodes { get; }

        public IObservable<ITaskRun> Execute(IObservable<NodeExecutionTask> Tasks)
        {
            return Observable.Create<ITaskRun>(async (o, ct) => await new ConsulRun(client) { Tasks = Tasks, Observer = o, Token = ct }.Run())
                .Publish().RefCount();
        }

        public void Dispose()
        {
            if (client.IsValueCreated)
                client.Value.Dispose();
        }


        private class ConsulRun
        {
            public ConsulRun(Lazy<ConsulClient> Client)
            {
                client = Client;
            }

            public IObservable<NodeExecutionTask> Tasks;
            public IObserver<TaskRun> Observer;
            public CancellationToken Token;

            public async Task<IDisposable> Run()
            {
                var tasksSubscription = SubscribeToTasksSource();

                try
                {
                    var firstLoop = true;
                    while (sessions.Any() || !taskSourceCompleted || nodeExecutionTasks.Any())
                    {
                        await RetryPolicy(async () => await Loop(firstLoop));
                        firstLoop = false;
                    }
                    Observer.OnCompleted();
                }
                catch (Exception e)
                {
                    Observer.OnError(e);
                }
                finally
                {
                    tasksSubscription.Dispose();
                    //Client.Dispose();
                }
                return Disposable.Empty;
            }

            private IDisposable SubscribeToTasksSource()
            {
                return Tasks.Subscribe(value =>
                {
                    lock (nodeExecutionTasks)
                    {
                        nodeExecutionTasks.Add(value);
                        tasksEnqueued.TrySetResult(true);
                    }
                }, () => taskSourceCompleted = true);
            }

            private async Task Loop(bool SkipInitialDelay)
            {
                var t2 = SkipInitialDelay ? Task.FromResult(true) : Task.Delay(1000, Token);
                await Task.WhenAny(tasksEnqueued.Task, t2);
                if (tasksEnqueued.Task.IsCompleted)
                    await StartEnqueuedTasks();
                else if (!Token.IsCancellationRequested)
                    foreach (var session in sessions)
                        await UpdateSessionState(session.Value, session.Key);
                foreach (var task in sessions.Keys)
                {
                    var counterValue = sessionCounters.GetOrAdd(task);
                    if (counterValue < LoopsBeforeExpectResult)
                        sessionCounters[task] = counterValue + 1;
                }
                var completed = sessions.Keys
                    .Where(s => GetNodesForTask(s).All(n => n.Value.IsCompleted)
                        && sessionCounters[s] == LoopsBeforeExpectResult
                    ).ToList();

                completed.ForEach(c => sessions.Remove(c));
            }

            private async Task StartEnqueuedTasks()
            {
                NodeExecutionTask[] todo;
                lock (nodeExecutionTasks)
                {
                    tasksEnqueued = new TaskCompletionSource<bool>();
                    todo = nodeExecutionTasks.ToArray();
                    nodeExecutionTasks.Clear();
                }

                foreach (var task in todo)
                {
                    var session = await CreateSession(Client, Token);
                    await FireJobEvent(Client, session, task, Token);
                    sessions[task] = session;
                }
            }

            private async Task UpdateSessionState(string Id, NodeExecutionTask ExecutionTask)
            {
                await RetryPolicy(() => Client.Session.Renew(Id, Token));
                var response = await RetryPolicy(() => RequestSessionKeys(Client, Id, Token));
                var acks = response.Where(v => v.Contains("ack")).Select(key => key.Split('/')[2]).ToArray();

                foreach (var ackNode in acks)
                {
                    var nodes = GetNodesForTask(ExecutionTask);
                    if (nodes.ContainsKey(ackNode))
                    {
                        foreach (var node in nodes.Values)
                        {
                            var nodeprefix = $"_rexec/{Id}/{node.NodeName}/";
                            node.CheckHeartbeat(heartbeatIndex =>
                            {
                                var key = $"{nodeprefix}out/{heartbeatIndex:00000}";
                                if (!response.Contains(key))
                                    return null;
                                var v = Client.KV.Get(key, Token).Result.Response.Value; //TODO async
                                return v == null ? "" : Cp866.GetString(v);
                            });

                            var exitKey = $"{nodeprefix}exit";
                            if (response.Contains(exitKey))
                            {
                                var retstr = (await Client.KV.Get(exitKey, Token)).Response.Value;
                                node.OnCompleted(Convert.ToInt32(Cp866.GetString(retstr)));
                            }
                        }
                    }
                    else
                    {
                        var taskRun = new TaskRun(ExecutionTask, ackNode);
                        Observer.OnNext(taskRun);
                        nodes[ackNode] = taskRun;
                    }
                }
            }

            private static readonly Encoding Cp866 = Encoding.GetEncoding("CP866");
            private const int LoopsBeforeExpectResult = 2;

            private readonly List<NodeExecutionTask> nodeExecutionTasks = new List<NodeExecutionTask>();
            private TaskCompletionSource<bool> tasksEnqueued = new TaskCompletionSource<bool>();
            private bool taskSourceCompleted;

            private ConsulClient Client => client.Value;
            private readonly Lazy<ConsulClient> client;

            private readonly Dictionary<NodeExecutionTask, string> sessions = new Dictionary<NodeExecutionTask, string>();
            private readonly Dictionary<NodeExecutionTask, int> sessionCounters = new Dictionary<NodeExecutionTask, int>();

            private readonly Dictionary<NodeExecutionTask, Dictionary<string, TaskRun>> taskruns =
                new Dictionary<NodeExecutionTask, Dictionary<string, TaskRun>>();

            private Task<T> RetryPolicy<T>(Func<Task<T>> Func) => RetryPatterns.Retry(Func, Token, TimeSpan.FromSeconds(1));
            private Task RetryPolicy(Func<Task> Func) => RetryPatterns.Retry(Func, Token, TimeSpan.FromSeconds(1));

            private IDictionary<string, TaskRun> GetNodesForTask(NodeExecutionTask SessionKey) => taskruns.GetOrAdd(SessionKey);
        }


        private static async Task<string[]> RequestSessionKeys(ConsulClient local, string session, CancellationToken ct)
        {
            return (await local.KV.Keys($"_rexec/{session}/", ct)).Response;
        }

        private static async Task FireJobEvent(ConsulClient local, string session, NodeExecutionTask task, CancellationToken ct)
        {
            await local.KV.Acquire(new KVPair($"_rexec/{session}/job")
            {
                Value = Encoding.UTF8.GetBytes("{\"Command\":\"" + task.Command + "\", \"Wait\":2000000000}"),
                Session = session
            }, ct);

            await Task.Delay(100, ct); //wait replication

            var msg = "{\"Prefix\":\"_rexec\", \"Session\":\"" + session + "\"}";

            await local.Event.Fire(new UserEvent
            {
                Name = "_rexec",
                Payload = Encoding.UTF8.GetBytes(msg),
                NodeFilter = task.NamePattern,
                ServiceFilter = task.ServicePattern,
                TagFilter = task.TagPattern
            }, ct);
        }

        private static async Task<string> CreateSession(ConsulClient local, CancellationToken ct)
        {
            var sessionReq = await local.Session
                .Create(new SessionEntry { TTL = TimeSpan.FromMinutes(1), Behavior = SessionBehavior.Delete }, ct);
            return sessionReq.Response;
        }

        private readonly Uri address;
        private readonly Lazy<ConsulClient> client;

        private async Task<ConsulClient> CreateClientAsync() => await Task.Run(() => CreateClient());

        private ConsulClient CreateClient() =>
            new ConsulClient(config => { config.Address = address; });
    }
}