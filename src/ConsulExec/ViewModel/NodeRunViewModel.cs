using System;
using System.Diagnostics;
using System.Reactive.Linq;
using ConsulExec.Domain;
using ConsulExec.Infrastructure;
using ReactiveUI;


namespace ConsulExec.ViewModel
{
    public enum NodeRunState { Added, Waiting, Writing, Heartbeat, Timeout, Completed }


    public class NodeRunViewModel : ReactiveObject
    {
        public NodeRunViewModel(string Name)
        {
            name = Observable.Return(Name).ToProperty(this, vm => vm.Name);
        }

        public NodeRunViewModel(IObservable<ITaskRun> TaskRun)
        {
            returnCode = TaskRun.SelectNSwitch(v => v.ReturnCode).Select(v => (int?)v).ToProperty(this, v => v.ReturnCode);
            output = TaskRun.SelectNSwitch(v => v.Output).Scan((a, s) => a + s).ToProperty(this, v => v.Output);
            name = TaskRun.Select(tr => tr.NodeName).ToProperty(this, vm => vm.Name);
            state = CreateStateProperty(TaskRun.SelectNSwitch(tr => tr.Output), TaskRun.SelectNSwitch(tr => tr.ReturnCode));
        }

        public string Name => name.Value;
        private readonly ObservableAsPropertyHelper<string> name;

        public NodeRunState State => state.Value;
        private readonly ObservableAsPropertyHelper<NodeRunState> state;

        public int? ReturnCode => returnCode.Value;
        private readonly ObservableAsPropertyHelper<int?> returnCode;

        public string Output => output.Value;
        private readonly ObservableAsPropertyHelper<string> output;

        private ObservableAsPropertyHelper<NodeRunState> CreateStateProperty(IObservable<string> Output, IObservable<int> ReturnCode)
        {
            var writing = Output.Where(v => !string.IsNullOrEmpty(v)).Heartbeat(TimeSpan.FromMilliseconds(2500)).StartWith(false)
                .Do(s => Debug.WriteLine("writing = " + s));

            var heartbeat = Output.Heartbeat(TimeSpan.FromMilliseconds(1500));
            var heartbeatOrWriting = heartbeat.CombineLatest(writing, (hb, w) => hb ? (w ? NodeRunState.Writing : NodeRunState.Heartbeat) : NodeRunState.Timeout);

            return heartbeatOrWriting
                .TakeUntil(ReturnCode)
                .StartWith(NodeRunState.Waiting)
                .Concat(Observable.Return(NodeRunState.Completed))
                .ToProperty(this, vm => vm.State);
        }
    }
}
