using System;
using System.Reactive.Subjects;

namespace ConsulExec.Domain
{
    class TaskRun : ITaskRun
    {
        public TaskRun(NodeExecutionTask Task, string NodeName)
        {
            this.Task = Task;
            this.NodeName = NodeName;
        }

        #region ITaskRun

        public NodeExecutionTask Task { get; }

        public string NodeName { get; }

        public IObservable<string> Output => output;

        public IObservable<int> ReturnCode => returnCode;

        #endregion

        public DateTime RecentOutput { get; private set; }

        public NodeExecState State;

        public bool IsCompleted { get; private set; }

        public DateTime RecentHeartbeat { get; private set; }

        public void OnCompleted(int Code)
        {
            IsCompleted = true;
            returnCode.OnNext(Code);
            returnCode.OnCompleted();
            output.OnCompleted();
        }

        public void CheckHeartbeat(Func<int, string> GetHeartbeatKey)
        {
            try
            {
                string s;
                var heartbeatReceived = false;
                var outputReceived = false;
                while ((s = GetHeartbeatKey(heartbeatCounter)) != null)
                {
                    if (s != "")
                    {
                        outputReceived = true;
                        output.OnNext(s);
                    }
                    heartbeatCounter++;
                    heartbeatReceived = true;
                }

                if (heartbeatReceived)
                {
                    RecentHeartbeat = DateTime.Now;
                    State = NodeExecState.Heartbeat;
                    if (outputReceived)
                        RecentOutput = RecentHeartbeat;
                }
                else if (DateTime.Now - RecentHeartbeat > TimeSpan.FromSeconds(10))
                    State = NodeExecState.Timeout;

            }
            catch (Exception)
            {
                State = NodeExecState.Unknown;
            }
        }

        private readonly ReplaySubject<string> output = new ReplaySubject<string>(2); //TODO how long buffer should be actually?

        private readonly AsyncSubject<int> returnCode = new AsyncSubject<int>();

        private int heartbeatCounter;
    }
}