using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using ConsulExec.Domain;
using ReactiveUI;
using Splat;

namespace ConsulExec.ViewModel
{
    public class CommandRunViewModel : ReactiveObject
    {
        public CommandRunViewModel()
        {
            if (!ModeDetector.InDesignMode())
                throw new InvalidOperationException("Design only constructor");
            NodeRuns = new[]
            {
                new NodeRunViewModel("N1"),
                new NodeRunViewModel("N2")
            };
        }

        public CommandRunViewModel(string[] EnqueuedNodeNames, IObservable<ITaskRun> TasksQueue, IActivatingViewModel Activator)
        {
            NodeRuns = EnqueuedNodeNames
                .Select(nodeName => new NodeRunViewModel(TasksQueue.Where(t => t.NodeName == nodeName).StartWith(new EnqueuedTaskRun(nodeName))))
                .ToArray();

            TasksQueue.Select(t => t.NodeName).Subscribe(curNode =>
            {
                if (!manualSelection)
                {
                    SelectedNodeRun = NodeRuns.FirstOrDefault(nr => nr.Name == curNode);
                    manualSelection = false;
                }
            }); //TODO where to dispose subscription

            var allCompleted = TasksQueue.SelectMany(t => t.ReturnCode.Select(_ => true)).All(b => b);
            isCompleted = allCompleted.ToProperty(this, vm => vm.IsCompleted);

            CloseCommand = ReactiveCommand.Create(() => Activator?.Deactivate(this), allCompleted.ObserveOnDispatcher());
        }

        public IEnumerable<NodeRunViewModel> NodeRuns { get; }

        public bool IsCompleted => isCompleted.Value;
        private readonly ObservableAsPropertyHelper<bool> isCompleted;

        public ICommand CloseCommand { get; }

        public NodeRunViewModel SelectedNodeRun
        {
            get { return selectedNodeRun; }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedNodeRun, value);
                manualSelection = true;
            }
        }
        private NodeRunViewModel selectedNodeRun;


        private class EnqueuedTaskRun : ITaskRun
        {
            public EnqueuedTaskRun(string NodeName)
            {
                this.NodeName = NodeName;
            }
            public NodeExecutionTask Task { get; }
            public string NodeName { get; }
            public IObservable<string> Output { get; } = Observable.Never<string>();
            public IObservable<int> ReturnCode { get; } = Observable.Never<int>();
        }

        private bool manualSelection;
    }
}